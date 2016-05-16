@Unit
Feature: Lose bet

Scenario: Losing a bet does not change balances
    Given I deposited $100
        And I place $40 bet
    When I lose $40 from bet
    Then Main balance should be $60
        And BetLost transaction should be created
        And 3 TransactionProcessed events should be sent

Scenario: Losing one of sub bets creates a transaction with proportional balance loss
    Given I deposited $150
        And I issued $150 bonus to Bonus balance
        And I place $100 bet
        And I place $100 bet
        And I place $100 bet
    When I lose $100 from bet
    Then BetLost transaction should be created
        And transaction main balance amount should be $-50
        And transaction bonus balance amount should be $-50