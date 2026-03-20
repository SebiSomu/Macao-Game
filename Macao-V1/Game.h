#pragma once
#include "Deck.h"
#include "Card.h"
#include "Player.h"
#include <map>

class Game
{
private:
    Deck m_deck;
    Player m_human;
    Player m_ai;
    std::stack<Card> m_discardPile;
    int m_cardsToDraw;
    bool m_skipNext;
    bool m_gameRunning;

    void dealInitialCards();
    void startGame();
    void playerTurn();
    void aiTurn();
    bool playCard(Player& player, Card& card);
    std::vector<Card> aiSelectCards();
    char aiChooseSuit();
    void reshuffleDeck();
    Card drawCardFromDeck(Player& player);
    char getUserSuitChoice();
    bool getUserContinueChoice(const std::string& value);
    void displayTopCard() const;
    void displayGameState() const;
    char getMostFrequentSuit(const std::vector<Card>& excludeCards) const;
    std::vector<Card> ensureValidFirstCard(const std::vector<Card>& selectedCards);

public:
    Game();
    void run();
};

