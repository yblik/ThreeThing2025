using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class blackjack : MonoBehaviour
{
    private List<string> deck;
    private List<string> playerHand;
    private List<string> dealerHand;
    private int playerScore;
    private int dealerScore;
    private bool gameActive;

    [Header("UI References")]
    public Text playerCardsText;
    public Text dealerCardsText;
    public Text gameStatusText;
    public GameObject playPrompt;
    public GameObject continuePrompt;

    [Header("Blackjack Settings")]
    public int minBet = 10;
    public int maxBet = 50;
    public int currentBet = 10;

    public ShopkeeperSpeech speech;

    // BANK REFERENCE
    private Bank bank;

    void Start()
    {
        bank = FindAnyObjectByType<Bank>();
        if (bank == null)
        {
            Debug.LogError("BANK NOT FOUND — add Bank script to scene.");
            enabled = false;
            return;
        }

        playPrompt?.SetActive(true);
        continuePrompt?.SetActive(false);

        gameStatusText.text = "Click to Play Blackjack!";
        gameActive = false;
        UpdateCardDisplays();
    }

    void Update()
    {
        int dinero = bank.Dinero; // always pull latest

        // --- ROUND NOT ACTIVE ---
        if (!gameActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (dinero < minBet)
                {
                    UpdateGameStatus("Not enough money to play!");
                    return;
                }

                StartNewGame();
            }

            if (Input.GetKeyDown(KeyCode.Space) && continuePrompt.activeSelf)
            {
                LeaveGame();
            }

            return;
        }

        // --- ACTIVE ROUND ---
        if (Input.GetMouseButtonDown(0)) PlayerHit();
        if (Input.GetKeyDown(KeyCode.Space)) PlayerStand();
    }

    // ===== START ROUND =====
    void StartNewGame()
    {
        int dinero = bank.Dinero;

        if (dinero < minBet)
        {
            UpdateGameStatus("Not enough money!");
            return;
        }

        // Deduct bet from Bank
        bank.Dinero -= currentBet;
        bank.SaveMoney();

        playPrompt?.SetActive(false);
        continuePrompt?.SetActive(false);
        gameActive = true;

        // Create deck
        deck = new List<string>();
        for (int i = 0; i < 4; i++)
            deck.AddRange(new List<string>() { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" });

        // Shuffle
        for (int i = 0; i < deck.Count; i++)
        {
            int r = Random.Range(i, deck.Count);
            (deck[i], deck[r]) = (deck[r], deck[i]);
        }

        playerHand = new List<string>();
        dealerHand = new List<string>();

        playerHand.Add(DrawCard());
        dealerHand.Add(DrawCard());
        playerHand.Add(DrawCard());
        dealerHand.Add(DrawCard());

        CalculateScores();
        UpdateCardDisplays();

        UpdateGameStatus($"Bet: £{currentBet} | Money: £{bank.Dinero}\nLEFT CLICK = HIT | SPACE = STAND");
    }

    // ===== END ROUND =====
    void EndGame()
    {
        gameActive = false;
        continuePrompt?.SetActive(true);

        UpdateGameStatus($"{gameStatusText.text}\nLEFT CLICK = Continue | SPACE = Leave");
        gameObject.SetActive(false);
    }

    void LeaveGame()
    {
        continuePrompt?.SetActive(false);
        playPrompt?.SetActive(true);

        UpdateGameStatus($"Game Ended. Money: £{bank.Dinero}\nLeft Click to Play Again");
    }

    // ===== CARD DRAWING =====
    string DrawCard()
    {
        string c = deck[0];
        deck.RemoveAt(0);
        return c;
    }

    void CalculateScores()
    {
        playerScore = CalculateHandValue(playerHand);
        dealerScore = CalculateHandValue(dealerHand);
    }

    int CalculateHandValue(List<string> hand)
    {
        int value = 0;
        int aces = 0;

        foreach (string card in hand)
        {
            if (card == "A") { value += 11; aces++; }
            else if ("JQK".Contains(card)) value += 10;
            else value += int.Parse(card);
        }

        while (value > 21 && aces > 0)
        {
            value -= 10;
            aces--;
        }

        return value;
    }

    // ===== PLAYER ACTIONS =====
    void PlayerHit()
    {
        if (!gameActive) return;

        playerHand.Add(DrawCard());
        CalculateScores();
        UpdateCardDisplays();

        if (playerScore > 21)
        {
            UpdateGameStatus($"BUST! You lose £{currentBet}");
            EndGame();
        }
        else
        {
            UpdateGameStatus($"Score: {playerScore} | HIT or STAND?");
        }
    }

    void PlayerStand()
    {
        if (!gameActive) return;

        UpdateGameStatus("Dealer is drawing...");

        while (dealerScore < 17)
        {
            dealerHand.Add(DrawCard());
            CalculateScores();
        }

        UpdateCardDisplays();

        bool win = false;
        bool push = false;
        string result;

        if (dealerScore > 21)
        {
            result = "Dealer busts! You win!";
            speech.Yap(0);

            win = true;
        }
        else if (dealerScore > playerScore)
        {
            result = "Dealer wins!";
            speech.Yap(1);

        }
        else if (playerScore > dealerScore)
        {
            result = "You win!";
            speech.Yap(0);
            win = true;
        }
        else
        {
            result = "Push! Bet returned.";
            speech.Yap(1);

            push = true;
        }

        // PAYOUT USING BANK
        if (win)
        {
            bank.Dinero += currentBet * 2;  // win + bet
        }
        else if (push)
        {
            bank.Dinero += currentBet; // bet returned
        }

        bank.SaveMoney();

        UpdateGameStatus(result);
        EndGame();
    }

    // ===== UI =====
    void UpdateCardDisplays()
    {
        playerCardsText.text = $"Player: {string.Join(" ", playerHand)}\nScore: {playerScore}";

        if (gameActive)
            dealerCardsText.text = $"Dealer: {dealerHand[0]} ?\nScore: ?";
        else
            dealerCardsText.text = $"Dealer: {string.Join(" ", dealerHand)}\nScore: {dealerScore}";
    }

    void UpdateGameStatus(string text)
    {
        gameStatusText.text = text;
        Debug.Log(text);
    }
}
