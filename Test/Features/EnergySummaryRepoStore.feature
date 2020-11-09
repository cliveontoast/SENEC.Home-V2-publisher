Feature: EnergySummaryRepoStore
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Persist when not yet persisted
	Given there is a energy summary for 2020/11/9
	And energy dependency injection is a thing
	And the energy summary has not been persisted
	And energy persistance is available
	When the energy summary is persisted 
	Then the energy summary should have been persisted

Scenario: Persist fails first time when not yet persisted
	Given there is a energy summary for 2020/11/9
	And energy dependency injection is a thing
	And the energy summary has not been persisted
	And energy persistance is available only on a retry
	When the energy summary is persisted 
	Then the energy summary should have been persisted the second time