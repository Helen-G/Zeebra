@Unit
Feature: Withdraw

Scenario: Withdraw decreases main balance
    Given I deposited $100
        And I apply $100 Withdrawal lock
    When I withdraw $100
    Then Main balance should be $0
        And Withdraw transaction should be created
        And 2 TransactionProcessed events should be sent
        And 2 LockApplied events should be sent

Scenario Outline: Can not withdraw invalid amount
    Given a wallet with no transactions
    When I withdraw $<amount>
    Then Invalid amount exception is thrown
Examples: 
    | amount |
    | 0      |
    | -100   |

Scenario: Can not withdraw more than free funds
    Given I deposited $100
        And I apply $50 Fraud lock
        And I apply $100 Withdrawal lock
    When I withdraw $100
    Then Insufficient funds exception is thrown