#include "Card.h"

Card::Card() : m_value(""), m_suit('?'), m_joker(false) {}

Card::Card(std::string_view m_cardstring) : m_value(""), m_suit('?'), m_joker(false)
{
    parseCardstring(m_cardstring);
}

bool Card::isJoker() const noexcept
{
    return m_joker; 
}

const std::string& Card::getValue() const noexcept
{
    return m_value;
}

char Card::getSuit() const noexcept
{
    return m_suit;
}

void Card::setSuit(char s) noexcept
{
    m_suit = s;
}

void Card::parseCardstring(std::string_view s)
{
    if (s == "Joker")
    {
        m_value = "Joker";   
        m_suit = '\0';        
        m_joker = true;
    }
    else if (s.size() == 3)
    {
        m_value = std::string(s.substr(0, 2));
        m_suit = s[2];
        m_joker = false;
    }
    else if (s.size() >= 2)
    {
        m_value = std::string(s.substr(0, 1));
        m_suit = s[1];
        m_joker = false;
    }
}

bool Card::isValidCard(const Card& topCard, int cardsToDraw) const
{
    if (cardsToDraw > 0)
        return (m_value == "2" || m_value == "3" || m_joker);

    if (m_joker || m_value == "7")
        return true;

    if (topCard.m_joker)
        return true;

    return (m_value == topCard.m_value || m_suit == topCard.m_suit);
}


bool Card::operator==(const Card& other) const noexcept
{
    return m_value == other.m_value && m_suit == other.m_suit;
}

std::ostream& operator<<(std::ostream& out, const Card& c)
{
    if (c.isJoker())
        out << "Joker ";
    else
        out << c.m_value << c.m_suit << " ";
    return out;
}

std::istream& operator>>(std::istream& in, Card& c)
{
    std::string s;
    in >> s;
    c.parseCardstring(s);
    return in;
}