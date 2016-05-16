@Unit
Feature: Lock

Scenario Outline: Locks from different domains increase appropriate lock
    Given I deposited $100
    When I apply $100 <lockName> lock
    Then <lockName> lock should be $100
        And $100 <lockName> lock record should be created
        And 1 LockApplied events should be sent
Examples: 
    | lockName   |
    | Fraud      |
    | Withdrawal |
    | Bonus      |

Scenario Outline: Can not lock invalid amount
    Given a wallet with no transactions
    When I apply $<amount> Fraud lock
    Then Invalid amount exception is thrown
Examples: 
    | amount |
    | 0      |
    | -100   |

Scenario: Can not lock more than total balance
    Given a wallet with no transactions
    When I apply $100 Fraud lock
    Then Insufficient funds exception is thrown