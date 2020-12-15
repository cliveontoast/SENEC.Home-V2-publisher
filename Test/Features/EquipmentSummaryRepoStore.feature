Feature: EquipmentStatesSummaryRepoStore
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Persist when not yet persisted
	Given there is a equipment states summary for 2020/11/9
	And equipment states dependency injection is a thing
	And the equipment states summary has not been persisted
	And equipment states persistance is available
	When the equipment states summary is persisted 
	Then the equipment states summary should have been persisted

Scenario: Persist fails first time when not yet persisted
	Given there is a equipment states summary for 2020/11/9
	And equipment states dependency injection is a thing
	And the equipment states summary has not been persisted
	And equipment states persistance is available only on a retry
	When the equipment states summary is persisted 
	Then the equipment states summary should have been persisted the second time
