@Unit
Feature: Cancel bet
    "Bet" is a short for Bet transaction.
    Bet transactions are:
        - BetPlaced
        - BetWon
        - BetLost

Scenario: Can not cancel non existent bet
    Given I deposited $100
		And I place $100 bet
    When I cancel the BetWon transaction
    Then application exception should be thrown

Scenario: Can not cancel same bet twice
    Given I deposited $100
        And I place $100 bet
        And I cancel the BetPlaced transaction
    When I cancel the BetPlaced transaction
    Then invalid operation exception should be thrown

Scenario Outline: Can cancel bet transactions only using cancel the bet call
    Given I deposited $100
        And <action>
    When I cancel the <trxName> transaction
    Then invalid operation exception should be thrown
Examples:
    | action                                                  | trxName      |
    | I deposited $100                                        | Deposit      |
    | I withdraw $100                                         | Withdraw     |
    | I issued $100 bonus to Bonus balance                    | Bonus        |

Scenario: Can cancel bet transactions only using cancel the bet call (multi-step)
    Given I deposited $100
        And I place $100 bet
        And I cancel the BetPlaced transaction
    When I cancel the BetCancelled transaction
    Then invalid operation exception should be thrown

Scenario: Canceling a bet can lead to negative balance
    Given I deposited $100
        And I place $100 bet
        And I win $120 from bet
        And I apply $100 Withdrawal lock
        And I withdraw $100
    When I cancel the BetWon transaction
    Then Main balance should be $-100
        And BetCancelled transaction should be created
        And transaction main balance amount should be $-120

Scenario: Canceling a bet, debited from both balances, is credited back proportionally
    Given I deposited $150
        And I issued $50 bonus to Bonus balance
        And I place $200 bet
    When I cancel the BetPlaced transaction
    Then Main balance should be $150
        And Bonus balance should be $50
        And BetCancelled transaction should be created
        And transaction main balance amount should be $150
        And transaction bonus balance amount should be $50
        And 4 TransactionProcessed events should be sent