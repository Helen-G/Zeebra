@Unit
Feature: Issue bonus

Scenario: Can issue bonus to main balance
    Given a wallet with no transactions
    When I issued $100 bonus to Main balance
    Then Main balance should be $100
        And Bonus transaction should be created
        And 1 TransactionProcessed events should be sent

Scenario: Can issue bonus to bonus balance
    Given a wallet with no transactions
    When I issued $100 bonus to Bonus balance
    Then Bonus balance should be $100
        And Bonus transaction should be created

Scenario Outline: Can not issue bonus of invalid amount
    Given a wallet with no transactions
    When I issued $<amount> bonus to Bonus balance
    Then Invalid amount exception is thrown
Examples: 
    | amount |
    | 0      |
    | -100   |