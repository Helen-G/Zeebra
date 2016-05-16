@Unit
Feature: Unlock

Scenario Outline: Unlock from specified domain decreases appropriate lock
    Given I deposited $100
        And I apply $100 <lockName> lock
    When I apply $50 <lockName> unlock
    Then <lockName> lock should be $50
Examples: 
    | lockName   |
    | Fraud      |
    | Withdrawal |
    | Bonus      |

Scenario: Unlock operation creates unlock transaction
    Given I deposited $100
        And I apply $100 Fraud lock
    When I apply $100 Fraud unlock
    Then $100 Fraud lock record should be created
        And 2 LockApplied events should be sent

Scenario Outline: Can not unlock invalid amount
    Given I deposited $100
        And I apply $100 Fraud lock
    When I apply $<amount> Fraud unlock
    Then Invalid amount exception is thrown
Examples: 
    | amount |
    | 0      |
    | -100   |

Scenario Outline: Can not unlock more than was locked
    Given I deposited $100
        And I apply $100 <lockName> lock
    When I apply $150 <lockName> unlock
    Then Invalid unlock amount exception is thrown
Examples: 
    | lockName   |
    | Fraud      |
    | Withdrawal |
    | Bonus      |