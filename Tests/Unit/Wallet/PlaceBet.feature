@Unit
Feature: Place bet

Scenario Outline: Can not place invalid amount bet
    Given a wallet with no transactions
    When I place $<amount> bet
    Then Invalid amount exception is thrown
Examples: 
    | amount |
    | 0      |
    | -100   |

Scenario: Can not place bet greater than playable balance
    Given I deposited $100
    When I place $200 bet
    Then Insufficient funds exception is thrown

Scenario: Placing bet creates transaction
    Given I deposited $150
        And I issued $50 bonus to Bonus balance
    When I place $200 bet
    Then BetPlaced transaction should be created
        And transaction main balance amount should be $-150
        And transaction bonus balance amount should be $-50
        And Main balance should be $0
        And Bonus balance should be $0
        And 3 TransactionProcessed events should be sent

Scenario: Can place several sub-bets
    Given I deposited $100
    When I place $50 bet
        And I place $50 bet
    Then 2 BetPlaced transactions should be created