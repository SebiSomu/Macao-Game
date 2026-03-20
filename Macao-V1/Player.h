#pragma once
#include "Card.h"
#include <vector>
#include <cstdint>
#include <string_view>

class Player
{
private:
    std::vector<Card> m_hand;
    std::uint8_t m_id;
    std::string m_name;

public:
    Player(int playerid, std::string playerm_name);
    void addCard(const Card& card);
    void addCard(Card&& card);
    bool removeCard(const Card& card);
    Card* findCard(const Card& card);
    std::uint8_t getId() const noexcept;
    const std::string& getName() const noexcept;
    const std::vector<Card>& getHand() const noexcept;
    std::vector<Card>& getHandRef() noexcept;
    std::uint8_t getHandSize() const noexcept;
    bool hasWon() const noexcept;
    void displayHand() const;
    bool hasCardWithValue(std::string_view m_value) const;
    std::vector<int> getIndicesOfValue(std::string_view m_value) const;
};

