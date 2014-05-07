-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- create survey table
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
if not exists (
	select 1 from sys.tables t
	inner join sys.schemas s on s.schema_id=t.schema_id
	where t.name='OsbideSurveys' and s.name='dbo')

	begin

	create table [dbo].[OsbideSurveys]
	(
		[SurveyResponseId] [int] not null,
		[Completed] [datetime],
		[LastPageSeen] [int],
		[StartLanguage] [nvarchar](255),
		[Token] [nvarchar](255),
		[Name] [nvarchar](255),
		[UserInstitutionId] [int],
		[Age] [int],
		[Gender_Gend] [nvarchar](255),
		[Ethnicity_Ethn] [nvarchar](255),
		[Class_Class] [nvarchar](255),
		[CSMajor1_CSMaj1] [nvarchar](255),
		[CSMajor2_CSMaj2] [nvarchar](255),
		[CsMajor3_CsMaj3] [nvarchar](255),
		[CsMajor4_CsMaj4] [nvarchar](255),
		[WhyCpt] [nvarchar](255),
		[CptS122_CptS122] [nvarchar](255),
		[CSE1_1] [nvarchar](255),
		[CSE1_2] [nvarchar](255),
		[CSE1_3] [nvarchar](255),
		[CSE1_4] [nvarchar](255),
		[CSE1_5] [nvarchar](255),
		[CSE2_6] [nvarchar](255),
		[CSE2_7] [nvarchar](255),
		[CSE2_8] [nvarchar](255),
		[CSE2_9] [nvarchar](255),
		[CSE2_10] [nvarchar](255),
		[CSE3_11] [nvarchar](255),
		[CSE3_12] [nvarchar](255),
		[CSE3_13] [nvarchar](255),
		[CSE3_14] [nvarchar](255),
		[CSE3_15] [nvarchar](255),
		[CSE4_16] [nvarchar](255),
		[CSE4_17] [nvarchar](255),
		[CSE4_18] [nvarchar](255),
		[CSE4_19] [nvarchar](255),
		[CSE4_20] [nvarchar](255),
		[CSE5_21] [nvarchar](255),
		[CSE5_22] [nvarchar](255),
		[CSE5_23] [nvarchar](255),
		[CSE5_24] [nvarchar](255),
		[CSE5_25] [nvarchar](255),
		[CSE5_26] [nvarchar](255),
		[CSE6_27] [nvarchar](255),
		[CSE6_28] [nvarchar](255),
		[CSE6_29] [nvarchar](255),
		[CSE6_30] [nvarchar](255),
		[CSE6_31] [nvarchar](255),
		[CSE6_32] [nvarchar](255),
		[CCS1_1] [nvarchar](255),
		[CCS1_2] [nvarchar](255),
		[CCS1_3] [nvarchar](255),
		[CCS1_4] [nvarchar](255),
		[CCS1_5] [nvarchar](255),
		[CCS2_6] [nvarchar](255),
		[CCS2_7] [nvarchar](255),
		[CCS2_8] [nvarchar](255),
		[CCS2_9] [nvarchar](255),
		[CCS2_10] [nvarchar](255),
		[CCS3_11] [nvarchar](255),
		[CCS3_12] [nvarchar](255),
		[CCS3_13] [nvarchar](255),
		[CCS3_14] [nvarchar](255),
		[CCS3_15] [nvarchar](255),
		[CCS4_16] [nvarchar](255),
		[CCS4_17] [nvarchar](255),
		[CCS4_18] [nvarchar](255),
		[CCS4_19] [nvarchar](255),
		[CCS4_20] [nvarchar](255),
		[mslq1_5] [nvarchar](255),
		[mslq1_6] [nvarchar](255),
		[mslq1_12] [nvarchar](255),
		[mslq1_15] [nvarchar](255),
		[mslq1_20] [nvarchar](255),
		[mslq1_21] [nvarchar](255),
		[mslq1_29] [nvarchar](255),
		[mslq1_31] [nvarchar](255),
		[mslq2_34] [nvarchar](255),
		[mslq2_45] [nvarchar](255),
		[mslq2_50] [nvarchar](255),
		[OsbideSurveyId] [int] not null identity primary key,
		[CourseId] [int] not null,
		[CreatedOn] datetime not null,
		[CreatedBy] int not null
	)

	end

-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
-- create student grade table
-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------
if not exists (
	select 1 from sys.tables t
	inner join sys.schemas s on s.schema_id=t.schema_id
	where t.name='StudentGrades' and s.name='dbo')

	begin

	create table [dbo].[StudentGrades]
	(
		[StudentGradeId] [int] not null identity primary key,
		[StudentId] [int] not null,
		[Deliverable] [nvarchar](255),
		[Grade] [decimal] not null,
		[CourseId] [int],
		[CreatedOn] datetime not null,
		[CreatedBy] int not null
	)

	end

	