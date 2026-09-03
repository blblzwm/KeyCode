/* ===== Subtopics ===== */
IF COL_LENGTH('Subtopics', 'isDeleted') IS NULL
BEGIN
    ALTER TABLE Subtopics
    ADD isDeleted BIT NOT NULL DEFAULT(0);
END;

IF COL_LENGTH('Subtopics', 'deleted_at') IS NULL
BEGIN
    ALTER TABLE Subtopics
    ADD deleted_at DATETIME NULL;
END;

IF COL_LENGTH('Subtopics', 'deleted_by') IS NULL
BEGIN
    ALTER TABLE Subtopics
    ADD deleted_by VARCHAR(50) NULL;
END;


/* ===== PracticeQuestions ===== */
IF COL_LENGTH('PracticeQuestions', 'isDeleted') IS NULL
BEGIN
    ALTER TABLE PracticeQuestions
    ADD isDeleted BIT NOT NULL DEFAULT(0);
END;

IF COL_LENGTH('PracticeQuestions', 'deleted_at') IS NULL
BEGIN
    ALTER TABLE PracticeQuestions
    ADD deleted_at DATETIME NULL;
END;