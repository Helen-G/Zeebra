@Unit
Feature: Wallet selection

Scenario: Can issue a bonus to the specific wallet
    Given a player wallet #1 with no transactions
        And a player wallet #2 with no transactions
    When I issued $100 bonus to wallet #2 Main balance
    Then wallet #2 Main balance should be $100
        And Bonus transaction should be created in wallet #2
        And wallet #1 Main balance should be $0