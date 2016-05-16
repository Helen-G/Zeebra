@Unit
Feature: Deposit

Scenario: Deposit increases main balance
    Given a wallet with no transactions
    When I deposited $100
    Then Main balance should be $100
        And Deposit transaction should be created
        And 1 TransactionProcessed events should be sent

Scenario Outline: Can not deposit invalid amount
    When I deposited $<amount>
    Then Invalid amount exception is thrown
Examples: 
    | amount |
    | 0      |
    | -100   |