#include "Game.h"
#include <random>
#include <algorithm>
#include <iterator>

Game::Game()
    : m_human(0, "You"), 
    m_ai(1, "AI"),
    m_cardsToDraw(0), 
    m_skipNext(false),
    m_gameRunning(true)
{    
}

void Game::dealInitialCards()
{
    const int INITIAL_CARDS = 5;
    for (int i = 0; i < INITIAL_CARDS; i++)
    {
        m_human.addCard(m_deck.drawCard());
        m_ai.addCard(m_deck.drawCard());
    }
}

void Game::startGame()
{
    m_discardPile.push(m_deck.drawCard());
}

void Game::displayTopCard() const
{
    std::cout << "Top card on discard pile: " << m_discardPile.top() << "\n";
}

void Game::displayGameState() const
{
    m_human.displayHand();
    displayTopCard();
}

char Game::getUserSuitChoice()
{
    char suit;
    std::cout << "You played a 7! Choose suit (T/R/N/C): ";
    std::cin >> suit;
    return suit;
}

bool Game::getUserContinueChoice(const std::string& value)
{
    std::cout << "Play another card with value " << value << "? (yes/no): ";
    std::string response;
    std::cin >> response;
    return response == "yes";
}

void Game::reshuffleDeck()
{
    if (!m_deck.isEmpty())
        return;

    std::cout << "Deck is empty, reshuffling discard pile...\n";

    Card lastCard = std::move(m_discardPile.top());
    m_discardPile.pop();

    while (!m_discardPile.empty())
    {
        m_deck.addCard(std::move(m_discardPile.top()));
        m_discardPile.pop();
    }

    m_deck.shuffle();
    m_discardPile.push(std::move(lastCard));
}

Card Game::drawCardFromDeck(Player& player)
{
    reshuffleDeck();
    Card drawn = m_deck.drawCard();
    player.addCard(drawn);
    return drawn;
}

bool Game::playCard(Player& player, Card& card)
{
    if (card.getValue() == "2") 
        m_cardsToDraw += 2;
    else if (card.getValue() == "3") 
        m_cardsToDraw += 3;
    else if (card.isJoker())
        m_cardsToDraw += 5;
    else if (card.getValue() == "A") 
        m_skipNext = true;
    else if (card.getValue() == "7")
    {
        char newSuit = (player.getId() == 0) ? getUserSuitChoice() : aiChooseSuit();
        card.setSuit(newSuit);
    }

    m_discardPile.push(card); // card is passed by reference, we copy into pile then remove from player
    player.removeCard(card);

    return true;
}

void Game::playerTurn()
{
    std::cout << "\n--- Your Turn ---\n";
    displayGameState();

    bool validPlay = false;
    while (!validPlay)
    {
        std::cout << "Choose card(s) to play or type 'draw': ";
        std::string input;
        std::cin >> input;

        if (input == "draw")
        {
            int drawCount = std::max(1, m_cardsToDraw);
            if (m_cardsToDraw > 1)
                std::cout << "Drawing " << drawCount << " cards.\n";

            for (int i = 0; i < drawCount; i++)
            {
                Card drawn = drawCardFromDeck(m_human);
                std::cout << "Drew: " << drawn << "\n";
            }
            m_cardsToDraw = 0;
            validPlay = true;
            continue;
        }

        Card chosenCard(input);
        Card* foundCard = m_human.findCard(chosenCard);

        if (!foundCard)
        {
            std::cout << "You don't have this card!\n";
            continue;
        }

        if (!foundCard->isValidCard(m_discardPile.top(), m_cardsToDraw))
        {
            std::cout << "Invalid card! Choose another.\n";
            continue;
        }

        playCard(m_human, *foundCard);

        while (m_human.hasCardWithValue(chosenCard.getValue()))
        {
            if (!getUserContinueChoice(chosenCard.getValue()))
                break;

            auto indices = m_human.getIndicesOfValue(chosenCard.getValue());
            std::cout << "Available cards with value " << chosenCard.getValue() << ":\n";
            for (int idx : indices)
                std::cout << idx << ": " << m_human.getHandRef()[idx] << "\n";

            std::cout << "Choose index: ";
            int chosen;
            std::cin >> chosen;

            if (std::find(indices.begin(), indices.end(), chosen) == indices.end())
            {
                std::cout << "Invalid index!\n";
                continue;
            }

            Card& nextCard = m_human.getHandRef()[chosen];
            playCard(m_human, nextCard);
        }

        validPlay = true;
    }
}

char Game::aiChooseSuit()
{
    std::map<char, int> suitFrequency;
    for (const auto& card : m_ai.getHand())
        suitFrequency[card.getSuit()]++;

    char bestSuit = 'T';
    int maxCount = -1;
    for (const auto& [suit, count] : suitFrequency)
        if (count > maxCount)
        {
            maxCount = count;
            bestSuit = suit;
        }
    
    if (maxCount <= 0 || (bestSuit != 'T' && bestSuit != 'R' && bestSuit != 'N' && bestSuit != 'C'))
    {
        const char suits[] = { 'T', 'R', 'N', 'C' };
        std::random_device rd;
        std::mt19937 g(rd());
        std::uniform_int_distribution<size_t> dist(0, 3);
        bestSuit = suits[dist(g)];
    }

    return bestSuit;
}

char Game::getMostFrequentSuit(const std::vector<Card>& excludeCards) const
{
    std::map<char, int> suitFrequency;
    
    for (const auto& card : m_ai.getHand())
    {
        bool isExcluded = false;
        for (const auto& excluded : excludeCards)
        {
            if (card == excluded)
            {
                isExcluded = true;
                break;
            }
        }
        
        if (!isExcluded && !card.isJoker() && card.getValue() != "7")
            suitFrequency[card.getSuit()]++;
    }

    char bestSuit = 'T';
    int maxCount = -1;
    for (const auto& [suit, count] : suitFrequency)
    {
        if (count > maxCount)
        {
            maxCount = count;
            bestSuit = suit;
        }
    }

    if (maxCount <= 0)
    {
        const char suits[] = { 'T', 'R', 'N', 'C' };
        std::random_device rd;
        std::mt19937 g(rd());
        std::uniform_int_distribution<size_t> dist(0, 3);
        bestSuit = suits[dist(g)];
    }

    return bestSuit;
}

std::vector<Card> Game::ensureValidFirstCard(const std::vector<Card>& selectedCards)
{
    if (selectedCards.empty())
        return {};

    Card topCard = m_discardPile.top();
    std::vector<Card> reordered = selectedCards;

    for (size_t i = 0; i < reordered.size(); ++i)
    {
        if (reordered[i].isValidCard(topCard, m_cardsToDraw))
        {
            if (i != 0)
                std::swap(reordered[0], reordered[i]);
            break;
        }
    }
    return reordered;
}


std::vector<Card> Game::aiSelectCards()
{
    Card topCard = m_discardPile.top();
    const auto& hand = m_ai.getHand();
    std::map<std::string, std::vector<Card>> validCardsByValue;

    if (m_cardsToDraw > 0)
    {
        for (const auto& card : hand)
        {
            if (card.getValue() == "2" || card.getValue() == "3" || card.isJoker())
                validCardsByValue[card.getValue()].push_back(card);
        }
    }
    else
    {
        for (const auto& card : hand)
            if (card.isValidCard(topCard, m_cardsToDraw))
                validCardsByValue[card.getValue()].push_back(card);
    }

    for (const auto& ace : hand)
    {
        if (ace.getValue() == "A" && ace.isValidCard(topCard, m_cardsToDraw))
        {
            for (const auto& other : hand)
                if (!(other == ace) && other.getSuit() == ace.getSuit())
                {
                    std::vector<Card> combo = { ace, other };
                    return combo;
                }
        }
    }

    if (validCardsByValue.empty())
    {
        if (m_cardsToDraw == 0)
        {
            for (const auto& c : hand)
                if (c.getValue() == "7")
                    return { c };

            for (const auto& c : hand)
                if (c.isJoker())
                    return { c };
        }

        return {};
    }

    std::map<std::string, std::vector<Card>> fullGroups;
    for (const auto& [value, validCards] : validCardsByValue)
    {
        std::vector<Card> allCardsOfValue;
        for (const auto& card : hand)
            if (card.getValue() == value)
                allCardsOfValue.push_back(card);

        fullGroups[value] = allCardsOfValue;
    }

    std::vector<Card> bestSelection;
    int bestScore = -1;

    for (auto& [value, cards] : fullGroups)
    {
        if (cards.empty())
            continue;

        bool isInflate = (value == "2" || value == "3" || cards[0].isJoker());

        if (isInflate && cards.size() > 1)
        {
            std::random_device rd;
            std::mt19937 g(rd());
            std::uniform_int_distribution<int> dist(0, 99);
            if (dist(g) >= 10)
            {
                std::vector<Card> single = { cards[0] };
                int score = 1;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestSelection = single;
                }
                continue;
            }
        }

        int score = (int)cards.size();
        if (score > bestScore)
        {
            bestScore = score;
            bestSelection = cards;
        }
    }

    if (bestSelection.size() > 1)
    {
        std::vector<Card> reordered;
        Card firstCard = bestSelection[0];
        bool foundMatch = false;

        for (const auto& card : bestSelection)
        {
            if (card.isValidCard(topCard, m_cardsToDraw))
            {
                firstCard = card;
                foundMatch = true;
                break;
            }
        }

        reordered.push_back(firstCard);
        for (const auto& card : bestSelection)
            if (!(card == firstCard))
                reordered.push_back(card);

        char bestSuit = getMostFrequentSuit(reordered);
        for (size_t i = 1; i < reordered.size(); i++)
        {
            if (reordered[i].getSuit() == bestSuit && !reordered[i].isJoker())
            {
                Card temp = reordered[i];
                reordered.erase(reordered.begin() + i);
                reordered.push_back(temp);
                break;
            }
        }

        bestSelection = reordered;
    }

    if (bestSelection.empty())
    {
        for (const auto& c : hand)
            if (c.isJoker())
                return { c };
    }

    if (!bestSelection.empty() && bestSelection[0].getValue() == "7")
    {
        char bestSuit = getMostFrequentSuit(m_ai.getHand());

        if (bestSelection.size() > 1)
        {
            std::stable_sort(bestSelection.begin() + 1, bestSelection.end(),
                [bestSuit](const Card& a, const Card& b)
                {
                    if (a.getSuit() == bestSuit && b.getSuit() != bestSuit)
                        return true;
                    if (a.getSuit() != bestSuit && b.getSuit() == bestSuit)
                        return false;
                    return a.getValue() < b.getValue();
                });
        }
    }

    return bestSelection;
}

void Game::aiTurn()
{
    std::cout << "\n--- AI Turn ---\n";
    //m_ai.displayHand();

    std::vector<Card> cardsToPlay = aiSelectCards();
    cardsToPlay = ensureValidFirstCard(cardsToPlay);

    if (!cardsToPlay.empty())
    {
        bool skipTriggered = false;

        for (auto it = cardsToPlay.begin(); it != cardsToPlay.end(); ++it)
        {
            Card* foundCard = m_ai.findCard(*it);
            if (!foundCard)
                continue;

            if (foundCard->getValue() == "7")
            {
                Card cardToPlay = *foundCard;
                char newSuit = aiChooseSuit();
                cardToPlay.setSuit(newSuit);
                std::cout << "AI played 7 and chose suit: " << newSuit << "\n";

                m_discardPile.push(std::move(cardToPlay));
                m_ai.removeCard(*foundCard);
                continue;
            }
            else if (foundCard->getValue() == "A")
            { 
                if (foundCard->isValidCard(m_discardPile.top(), m_cardsToDraw))
                {
                    m_skipNext = true;
                    std::cout << "AI played: " << *foundCard << "\n";
                    m_discardPile.push(*foundCard);
                    m_ai.removeCard(*foundCard);
                    skipTriggered = true;
                    break; 
                }
                else
                    continue;
            }
            else if (foundCard->getValue() == "2")
            {
                m_cardsToDraw += 2;
            }
            else if (foundCard->getValue() == "3")
            {
                m_cardsToDraw += 3;
            }
            else if (foundCard->isJoker())
            {
                m_cardsToDraw += 5;
            }

            m_discardPile.push(*foundCard);
            m_ai.removeCard(*foundCard);
        }

        if (!skipTriggered)
        {
            std::cout << "AI played: ";
            for (const auto& card : cardsToPlay)
                std::cout << card;
            std::cout << "\n";
        }
        std::cout << "AI has " << m_ai.getHand().size() << " card(s) left in hand.\n";
    }
    else
    {
        int drawCount = std::max(1, m_cardsToDraw);
        for (int i = 0; i < drawCount; i++)
            Card drawn = drawCardFromDeck(m_ai);

        if (drawCount == 1)
            std::cout << "AI drew " << drawCount << " card.\n";
        else
            std::cout << "AI drew " << drawCount << " card(s).\n";

        std::cout << "AI now has " << m_ai.getHand().size() << " card(s) in hand.\n";

        m_cardsToDraw = 0;
    }
}

void Game::run()
{
    dealInitialCards();
    startGame();

    while (m_gameRunning)
    {
        if (!m_skipNext)
        {
            playerTurn();
            if (m_human.hasWon())
            {
                std::cout << "You won!\n";
                break;
            }
        }
        else
        {
            std::cout << "Your turn is skipped due to an Ace!\n";
            m_skipNext = false;
        }

        if (!m_skipNext)
        {
            aiTurn();
            if (m_ai.hasWon())
            {
                std::cout << "AI won!\n";
                m_gameRunning = false;
            }
        }
        else
        {
            std::cout << "AI's turn is skipped due to an Ace!\n";
            m_skipNext = false;
        }
    }
}