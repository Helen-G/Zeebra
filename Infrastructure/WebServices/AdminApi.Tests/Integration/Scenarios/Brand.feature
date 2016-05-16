@Integration

Feature: Brand
    As a user, I can add, view, edit, activate, deactivate brand and countries assigned to it

Scenario: Get user brands
    Given I am logged in and have access token
    Then Available brands are visible to me

Scenario: Get add brand data
    Given I am logged in and have access token
    Then Required data to add new brand is visible to me

Scenario: Get edit brand data
    Given I am logged in and have access token
    When New activated brand is created
    Then Required data to edit that brand is visible to me

Scenario: Get view brand data
    Given I am logged in and have access token
    When New activated brand is created
    Then Required brand data is visible to me

Scenario: Add new brand
    Given I am logged in and have access token
    Then New brand is successfully added

Scenario: Edit brand data
    Given I am logged in and have access token
    When New deactivated brand is created
    Then Brand data is successfully edited

Scenario: Get brand countries
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand countries are visible to me

Scenario: Activate brand
    Given I am logged in and have access token
    When New deactivated brand is created
    Then Brand is successfully activated

Scenario: Deactivate brand
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand is successfully deactivated

Scenario: Get brands list
    Given I am logged in and have access token
    When New activated brand is created
    Then Brands are visible to me

Scenario: Get brands countries assign data
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand country assign data is visible to me

#Scenario: Get brands countries list
#    Given I am logged in and have access token
#    When New activated brand is created
#    Then Brands countries list is visible to me

Scenario: Assign country to the brand
    Given I am logged in and have access token
    When New activated brand is created
        And New country is created
    Then Brand country is successfully added

Scenario: Get brands cultures assign data
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand culture assign data is visible to me

#Scenario: Get brands cultures list
#    Given I am logged in and have access token
#    When New activated brand is created
#    Then Brands cultures list is visible to me

Scenario: Assign culture to the brand
    Given I am logged in and have access token
    When New activated brand is created
        And New culture is created
    Then Brand culture is successfully added

Scenario: Get brand currencies
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand currencies are visible to me

Scenario: Get brand currency assign data
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand currency assign data is visible to me

Scenario: Get brand product assign data
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand product assign data is visible to me

Scenario: Assign product to the brand
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand product is successfully assigned

Scenario: Get brand product bet levels
    Given I am logged in and have access token
    When New activated brand is created
    Then Brand product bet levels are visible to me

Scenario: Create new content translation
    Given I am logged in and have access token
    When New culture is created
    Then New content translation is successfully created

Scenario: Can not execute permission protected brand methods
    Given I am logged in and have access token
    Then I can not execute protected brand methods with insufficient permissions