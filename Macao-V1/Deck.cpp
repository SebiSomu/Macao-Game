#include "Deck.h"
#include <vector>
#include <random>
#include <algorithm>

Deck::Deck()
{
    loadFromFile();
    shuffleArray();
    cut();

    for (auto& card : m_cards)
        m_drawPile.push(std::move(card));
}

bool Deck::isEmpty() const
{
    return m_drawPile.empty();
}

void Deck::loadFromFile(const std::string& fileName)
{
    std::ifstream fin(fileName);
    std::string m_cardstring;
    int i = 0;

    while (fin >> m_cardstring && i < 54)
    {
        m_cards[i] = Card(m_cardstring);
        i++;
    }
}

void Deck::shuffleArray()
{
    std::random_device rd;
    std::mt19937 g(rd());
    std::shuffle(m_cards.begin(), m_cards.end(), g);
}

void Deck::shuffle()
{
    std::vector<Card> temp;
    temp.reserve(m_drawPile.size());
    while (!m_drawPile.empty())
    {
        temp.push_back(std::move(m_drawPile.top()));
        m_drawPile.pop();
    }
    
    std::random_device rd;
    std::mt19937 g(rd());
    std::shuffle(temp.begin(), temp.end(), g);
    
    for (auto& card : temp)
        m_drawPile.push(std::move(card));
}

void Deck::cut()
{
    std::random_device rd;
    std::mt19937 g(rd());
    std::uniform_int_distribution<size_t> dist(0, m_cards.size() - 1);
    size_t cutPoint = dist(g);
    std::rotate(m_cards.begin(), m_cards.begin() + cutPoint, m_cards.end());
}

Card Deck::drawCard()
{
    if (m_drawPile.empty())
        throw std::runtime_error("Draw pile is empty!");

    Card drawn = m_drawPile.top();
    m_drawPile.pop();
    return drawn;
}

void Deck::addCard(const Card& card)
{
    m_drawPile.push(card);
}

void Deck::addCard(Card&& card)
{
    m_drawPile.push(std::move(card));
}

std::uint8_t Deck::size() const noexcept
{
    return static_cast<std::uint8_t>(m_drawPile.size());
}


