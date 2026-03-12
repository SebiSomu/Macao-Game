#pragma once
#include "Card.h"
#include <array>
#include <stack>
#include <fstream>
#include <algorithm>
#include <cstdlib>
#include <ctime>
#include <cstdint>
#include <random>

class Deck
{
private:
    std::array<Card, 54> m_cards;
    std::stack<Card> m_drawPile;
    void loadFromFile(const std::string& fileName = "cards_package.txt");
    void shuffleArray();
    void cut();

public:
    Deck();
    bool isEmpty() const;
    Card drawCard();
    void addCard(const Card& card);
    void addCard(Card&& card);
    std::uint8_t size() const noexcept;
    void shuffle();
};
