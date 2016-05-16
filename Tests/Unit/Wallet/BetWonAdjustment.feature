@Unit
Feature: Bet won adjustment

Scenario: Winning from betting during rollover is adjusted to bonus balance
    Given I deposited $100
        And wallet has active wagering requirement
        And I place $100 bet
        And I win $120 from bet
    When system adjusts transaction to be during rollover
    Then Main balance should be $0
        And Bonus balance should be $120
        And BetWonAdjustment transaction should be created
        And transaction main balance amount should be $0
        And transaction bonus balance amount should be $120
        And transaction temporary balance amount should be $-120
        And 4 TransactionProcessed events should be sent

Scenario: Winning from bet, that was placed using both balances, is adjusted back proportionally
    Given I deposited $150
        And wallet has active wagering requirement
        And I issued $50 bonus to Bonus balance
        And I place $200 bet
        And I win $1000 from bet
    When system adjusts transaction to not be during rollover
    Then Main balance should be $750
        And Bonus balance should be $250
        And BetWonAdjustment transaction should be created
        And transaction main balance amount should be $750
        And transaction bonus balance amount should be $250

Scenario: Winning from bet, that has several sub bets, is adjusted back proportionally
    Given I deposited $150
        And wallet has active wagering requirement
        And I issued $150 bonus to Bonus balance
        And I place $100 bet
        And I place $100 bet
        And I place $100 bet
        And I win $1000 from bet
    When system adjusts transaction to not be during rollover
    Then Main balance should be $500
        And Bonus balance should be $500

Scenario: Adjusting a bet that was canceled is ignored
    Given I deposited $100
        And wallet has active wagering requirement
        And I place $100 bet
        And I win $120 from bet
        And I cancel the BetWon transaction
    When system adjusts transaction to be during rollover
    Then no BetWonAdjustment transaction should be created