@Unit
Feature: Win bet

Scenario Outline: Can not win invalid amount
    Given I deposited $100
        And I place $100 bet
    When I win $<amount> from bet
    Then Invalid amount exception is thrown
Examples: 
    | amount |
    | 0      |
    | -100   |

Scenario: Winning from bet, that was placed using both balances, is credited back proportionally
    Given I deposited $150
        And I issued $50 bonus to Bonus balance
        And I place $200 bet
    When I win $1000 from bet
    Then Main balance should be $750
        And Bonus balance should be $250
        And BetWon transaction should be created
        And transaction main balance amount should be $750
        And transaction bonus balance amount should be $250
        And 4 TransactionProcessed events should be sent

Scenario: Winning from bet, that has several sub bets, is credited back proportionally
    Given I deposited $150
        And I issued $150 bonus to Bonus balance
        And I place $100 bet
        And I place $100 bet
        And I place $100 bet
    When I win $1000 from bet
    Then Main balance should be $500
        And Bonus balance should be $500

Scenario: Winning from bet during active wagering requirement goes to temporary balance
    Given I deposited $100
        And wallet has active wagering requirement
        And I place $100 bet
    When I win $200 from bet
    Then BetWon transaction should be created
        And transaction temporary balance amount should be $200