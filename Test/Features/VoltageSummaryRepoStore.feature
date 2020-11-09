Feature: VoltageSummaryRepoStore
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Persist when not yet persisted
	Given there is a voltage summary for 2020/11/9
	And dependency injection is a thing
	And the voltage summary has not been persisted
	And persistance is available
	When the voltage summary is persisted 
	Then the voltage summary should have been persisted

Scenario: Persist fails first time when not yet persisted
	Given there is a voltage summary for 2020/11/9
	And dependency injection is a thing
	And the voltage summary has not been persisted
	And persistance is available only on a retry
	When the voltage summary is persisted 
	Then the voltage summary should have been persisted the second time