@Unit
Feature: Adjust balances
    Bonus domain uses balance adjustments in such cases:
        - BonusCancelled: Reverts bonus reward funds and any net win made by bonus reward funds
        - WageringFinished: Transfers funds won during wagering requirement to main balance

Scenario Outline: Balance adjustments change balances
    Given I deposited $100
        And I issued $100 bonus to Bonus balance
    When system adjusts Main for -50$, Bonus for -50$, Temporary for 100$ because of <reasonName> reason
    Then Main balance should be $50
        And Bonus balance should be $50
        And <reasonName> transaction should be created
        And transaction main balance amount should be $-50
        And transaction bonus balance amount should be $-50
        And transaction temporary balance amount should be $100
        And 3 TransactionProcessed events should be sent
Examples: 
    | reasonName       |
    | BonusCancelled   |
    | WageringFinished |

Scenario Outline: Bonus and Main balances can not be adjusted to be negative
    Given a wallet with no transactions
    When system adjusts Main for 0$, Bonus for <bonusAdjustment>$, Temporary for <tempAdjustment>$ because of BonusCancelled reason
    Then Insufficient funds exception is thrown
Examples: 
    | bonusAdjustment | tempAdjustment |
    | -50             | 0              |
    | 0               | -50            |