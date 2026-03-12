#pragma once
#include <iostream>
#include <string>
#include <string_view>
#include <ranges>
#include <algorithm>

class Card
{
public:
    Card();
    Card(std::string_view m_cardstring);
    Card(const Card& other) = default;
    Card(Card&& other) noexcept = default;
    Card& operator=(const Card& other) = default;
    Card& operator=(Card&& other) noexcept = default;
    ~Card() = default;

    bool isJoker() const noexcept;
    const std::string& getValue() const noexcept;
    char getSuit() const noexcept;
    void setSuit(char s) noexcept;
    bool isValidCard(const Card& lastCard, int m_cardsToDraw) const;
    bool operator==(const Card& other) const noexcept;
    friend std::ostream& operator<<(std::ostream& out, const Card& c);
    friend std::istream& operator>>(std::istream& in, Card& c);

private:
    std::string m_value;
    char m_suit;
    bool m_joker;
    void parseCardstring(std::string_view s);
};