#include "Player.h"

#include <iostream>
#include <algorithm>

Player::Player(int playerid, std::string playerm_name)
    : m_id(static_cast<std::uint8_t>(playerid)), m_name(std::move(playerm_name)) {
}

void Player::addCard(const Card& card)
{
    m_hand.push_back(card);
}

void Player::addCard(Card&& card)
{
    m_hand.push_back(std::move(card));
}

bool Player::removeCard(const Card& card)
{
    auto it = std::find(m_hand.begin(), m_hand.end(), card);
    if (it != m_hand.end())
    {
        m_hand.erase(it);
        return true;
    }
    return false;
}

Card* Player::findCard(const Card& card)
{
    auto it = std::find(m_hand.begin(), m_hand.end(), card);
    if (it != m_hand.end())
        return &(*it);
    return nullptr;
}

std::uint8_t Player::getId() const noexcept
{
    return m_id;
}

const std::string& Player::getName() const noexcept
{
    return m_name;
}

const std::vector<Card>& Player::getHand() const noexcept
{
    return m_hand;
}

std::vector<Card>& Player::getHandRef() noexcept
{
    return m_hand;
}

std::uint8_t Player::getHandSize() const noexcept
{
    return static_cast<std::uint8_t>(m_hand.size());
}

bool Player::hasWon() const noexcept
{
    return m_hand.empty();
}

void Player::displayHand() const
{
    if (m_name == "You")
        std::cout << "Your cards:\n";
    else
        std::cout << m_name << "'s cards:\n";
    for (const auto& card : m_hand)
        std::cout << card;
    std::cout << "\n";
}

bool Player::hasCardWithValue(std::string_view m_value) const
{
    for (const auto& card : m_hand)
        if (card.getValue() == m_value)
            return true;
    return false;
}

std::vector<int> Player::getIndicesOfValue(std::string_view m_value) const
{
    std::vector<int> indices;
    for (int i = 0; i < (int)m_hand.size(); i++)
        if (m_hand[i].getValue() == m_value)
            indices.push_back(i);
    return indices;
}
