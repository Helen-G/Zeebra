@Integration
Feature: Core Game Api
	Calling Core Game API 

Scenario: Get and validate authentication token
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
	When I call to validate token
		Then I will receive successful validation result

Scenario: Get playable balance
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $11 and bonus balance is $3
	When I get balance
		Then the player's playable balance will be $14

Scenario: Place bet when playable balance is $0 
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $0 and bonus balance is $0
	When I bet $5
		Then I will get error code "InsufficientFunds"
	When I get balance
		Then the player's playable balance will be $0
		# in other words the balance will not be changed 
		And requested bet will not be recorded	
		And place bet response balance will equal requested balance

Scenario: Place bet when playable balance is insufficient
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $10 and bonus balance is $0
	When I bet $15
		Then I will get error code "InsufficientFunds"
	When I get balance
		Then the player's playable balance will be $10
		# in other words the balance will not be changed
		And requested bet will not be recorded	
		And place bet response balance will equal requested balance

Scenario: Place bet when playable balance is sufficient
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $10 and bonus balance is $10
	When I bet $15
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $5
		And requested bet will be recorded	
		And place bet response balance will equal requested balance
	
Scenario: Place and win bet
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $20 and bonus balance is $0
	When I bet $15
		Then I will get error code "NoError"
	When I win $25
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $30
		And requested bet will be recorded

Scenario: Place and "win" less than what was placed
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $20 and bonus balance is $0
	When I bet $15
		Then I will get error code "NoError"
	When I win $5
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $10
		And requested bet will be recorded

Scenario: Place and lose bet
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $20 and bonus balance is $0
	When I bet $15
		Then I will get error code "NoError"
	When I lose the bet
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $5
		And requested bet will be recorded

Scenario: Get free bet
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $20 and bonus balance is $0
	When I get free bet for $15
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $35
		And requested bet will be recorded

Scenario: Settle batch of bets
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $20 and bonus balance is $0
	When I place bets for amount:
		| Amount|
		| 5.0   |
		| 5.0   |
		| 5.0   |
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $5
	When I settle the following bets:
		| Type | Amount |
		| WIN  | 400.0   |
		| LOSE |  0.0   |
		| WIN  |  32.0   |
		# note that lose bet must always be of amount=0
		Then I will get error code "NoError"
	When I get balance
		# expect balance: 20 - 5 - 5 - 5 + 400 + 32 = 437
		Then the player's playable balance will be $437
		And requested bets will be recorded

Scenario: Getting bet history
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $100 and bonus balance is $0
	When I bet $15
		Then I will get error code "NoError"
	When I bet $5
		Then I will get error code "NoError"
	When I get history
		Then I will see the bet IDs in the history

Scenario: Cancel transaction of placing bet
	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
		And I validate the token
		And the player "testplayer" main balance is $100 and bonus balance is $0
	When I bet $15
		Then I will get error code "NoError"
	When I cancel the last transaction
		Then I will get error code "NoError"
	When I get balance
		Then the player's playable balance will be $100

#
# UNCOMMENT WHEN WALLET ADJUST IS DONE		
#
#Scenario: Adjust transaction by adding amount
#	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
#		And I validate the token
#		And the player "testplayer" main balance is $100 and bonus balance is $0
#	When I bet $15
#		Then I will get error code "NoError"
#	When I adjust transaction with $5
#		Then I will get error code "NoError"
#	When I get balance
#		# 100 - 15 + 5 = 90
#		Then the player's playable balance will be $90
#
#Scenario: Adjust transaction by removing amount
#	Given I get authentication token for player "testplayer" with password "123456" for game "Roulette"
#		And I validate the token
#		And the player "testplayer" main balance is $100 and bonus balance is $0
#	When I bet $15
#		Then I will get error code "NoError"
#	When I adjust transaction with $-5
#		Then I will get error code "NoError"
#	When I get balance
#		# 100 - 15 - 5 = 80
#		Then the player's playable balance will be $80
#