@Integration

Feature: Admin
    As a user, I can manage country, culture, currency

# Country

#Scenario: Get countries list
#    Given I am logged in and have access token
#    Then Available countries are visible to me

Scenario: Get country by code
    Given I am logged in and have access token
    When New country is created
    Then Country by code is visible to me

Scenario: Save country data
    Given I am logged in and have access token
    Then Country data is successfully saved

Scenario: Delete country
    Given I am logged in and have access token
    When New country is created
    Then Country is successfully deleted

# Culture

#Scenario: Get cultures list
#    Given I am logged in and have access token
#    Then Available cultures are visible to me

Scenario: Get culture by code
    Given I am logged in and have access token
    When New culture is created
    Then Culture by code is visible to me

Scenario: Activate culture
    Given I am logged in and have access token
    When New culture is created
    Then Culture is successfully activated

Scenario: Deactivate culture
    Given I am logged in and have access token
    When New culture is created
    Then Culture is successfully deactivated

Scenario: Save culture data
    Given I am logged in and have access token
    Then Culture data is successfully saved

# Currency

#Scenario: Get currencies list
#    Given I am logged in and have access token
#    Then Available currencies are visible to me

Scenario: Get currency by code
    Given I am logged in and have access token
    When New currency is created
    Then Currency by code is visible to me

Scenario: Activate currency
    Given I am logged in and have access token
    When New currency is created
    Then Currency is successfully activated

Scenario: Deactivate currency
    Given I am logged in and have access token
    When New currency is created
    Then Currency is successfully deactivated

Scenario: Save currency data
    Given I am logged in and have access token
    Then Currency data is successfully saved

# Common

Scenario: Can not execute permission protected admin methods
    Given I am logged in and have access token
    Then I can not execute protected admin methods with insufficient permissions