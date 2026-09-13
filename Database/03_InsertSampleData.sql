-- KeyCode Sample Data
-- Insert sample data for testing (optional)

USE KeyCodeDB;
GO

-- Insert Sample Users
INSERT INTO Users
(
    userID,
    username,
    fname,
    lname,
    pwd_hash,
    email,
    dob,
    role,
    qualification,
    status,
    reset_token,
    reset_token_expiry,
    upload_profile,
    security_question,
    security_ans_hash,
    google_subject
)
VALUES
('A001','yuqi','Song','Yu Qi','$argon2id$v=19$m=65536,t=3,p=1$4kT4TtBp+TQZxgSKMyhoeg$IKCoPjUeCWoWJWLwdN4Wgb33OmqtTI03IZduKBdv0Lg','gidle@gmail.com','1999-09-23','Admin',NULL,'Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('A002','sana','Sana','Minatozaki','$argon2id$v=19$m=65536,t=3,p=1$VDF72dIvxFzwXEwy2NFrug$5mEsTYvoxq/7jIgzXdhyeGam2tjcnJ+O6EhpcDFsY4M','sana.keycode@hotmail.com','1996-02-29','Admin',NULL,'Active','ebd50a87ae80fa2e0e47394e71d3ccf086982c22a9f16590600196d0e96b8de8','2026-09-09 16:30:14.307','~/Uploads/UserProfile/sana_profile_pic.jpg','What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('A003','tzuyu','Tzuyu','Chou','$argon2id$v=19$m=65536,t=3,p=1$8S0QfGP+1UmHOehKySZE7A$YbIoQWd/9PnYpe38znn6U69ppJF4MjxssssdQR59n80','tzuyu@admin.keycode.com','1999-06-14','Admin',NULL,'Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('S001','leehan','Lee','Han',NULL,'leehan001@hotmail.com','2004-10-18','Student',NULL,'Suspended',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('S002','bibiannezy','Bibianne','Zheyee','$argon2id$v=19$m=65536,t=3,p=1$GEoLAsa90vHerTXTSirI+A$TDLR3ar8IVojknW7WQkS4CIhRF/bDGL34kbm+p+pfgQ','bibianne@gmail.com','2005-03-20','Student',NULL,'Active','9a704298-c84e-4fba-9c35-b92692679d00','2026-03-01 18:32:42.000','~/Uploads/UserProfile/45f3703b-ac69-4928-8e2e-1901da6b1ba0.png','What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('S003','jennierubyjane','Jennie','Ruby','$argon2id$v=19$m=65536,t=3,p=1$Uu/Gxh3YyWURrzFrB/rcdg$bUpvvdJP2WL/Vu6oKRtjDbNZC/8tFRBu9pl///Owrqg','jennieruby@gmail.com','1997-01-17','Student',NULL,'Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('S004','aerichandesu','Giselle','Ri','$argon2id$v=19$m=65536,t=3,p=1$1uhD1Yyyax9MX5zFY7H0lA$wA/lXXAXb9fOa/GvAAfGwZ6aokIrzmAI+wKDKaoKwS4','giselle@gmail.com','2000-10-25','Student',NULL,'Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('S005','soobin','Choi','Soo Bin','$argon2id$v=19$m=65536,t=3,p=1$cwWTuM48ewiNkxno/nEd7w$DmfdAH47rx2AhyZ5bbh0v4vKhAYPY7d6WN2RkYE1g60','sb1205@gmail.com','2000-12-05','Student',NULL,'Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('S006','tevy','Tevy','Chong','$argon2id$v=19$m=65536,t=3,p=1$f62GsllIDsIbDFYK83gT0g$KteAAjyrLDtiL7x4DZRcI8L1KtHuVsaO5NCZY5QwC+s','tevy@gmail.com','2005-07-15','Student',NULL,'Deleted',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('S007','myung','Jaehyun','Myung','$argon2id$v=19$m=65536,t=3,p=1$qYpsbQPBd47i06/OXhumoQ$Hq//5kx+qbPv3hNJWW5T12MW1DK47DvJtmiB+Ki1PGw','myung@gmail.com','2003-12-13','Student',NULL,'Active',NULL,NULL,'~/Uploads/UserProfile/d46236f0-0871-443d-b35b-d133cc43c793.jpg','What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$fFhjJg9NOTO8xR9VewRqYQ$brLiUXJtHTAZU1J52di5lvyJey0r6XPuP5ljb7FN9Qw',NULL),
('S008','taesan','Taesan','Han','$argon2id$v=19$m=65536,t=3,p=1$ZwF3qPJvnyvYB7SmYUbWXg$w+vM7r24xYKfVxxaomypimZZ+IFTQYLnaF445iEN24E','taesan@gmail.com','2004-08-13','Student',NULL,'Active',NULL,NULL,NULL,'What was the name of your first pet?','$argon2id$v=19$m=65536,t=3,p=1$8Nhyne9oHijFh0BSmpJPEA$zOD7onfhCJ+NYJFa2Uz7i9xag/iqs4CAXaZVIVzflwo',NULL),
('S009','cxcx68466','Joshua','Yang',NULL,'cxcx68466@gmail.com','2011-01-06','Student',NULL,'Active',NULL,NULL,'~/Uploads/UserProfile/1fcfb064-7b26-42c1-b2d8-170c4c241df9.png',NULL,NULL,'113615345903093276900'),
('T001','minji','Kim','Min Ji','$argon2id$v=19$m=65536,t=3,p=1$V7ppW3JA6bJnSL68pjLAAw$81Xd+vMoRXE+8O2lwJROEZMZjTN4RBTsZ4s6CNQ0Aag','njz@gmail.com','2004-05-07','Tutor','Master','Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('T002','ong','Ong','Li Xuan','$argon2id$v=19$m=65536,t=3,p=1$wzoiEt4LkzCsBuusbVo8QA$/HXdrafTm7EHj/8bKa4gDPtXx33nniiS/Dge42oaRUY','ong@gmail.com','2005-08-20','Tutor','Master','Active',NULL,NULL,'~/Uploads/77a542d5-f24a-4286-a735-ef5210a52b2b.png','What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('T003','mokmok','Mok','Heng Xuan','$argon2id$v=19$m=65536,t=3,p=1$aE+fQgwhZql6RlbL6XsH7A$8p8sx64QHwk7RKYyjSA4JnuBJc3e5zEDwD6I57WFgg4','mokmok@gmail.com','2000-01-01','Tutor','Master','Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('T004','Nana','Nana','Osaki','$argon2id$v=19$m=65536,t=3,p=1$r/ZiTn0B0lv/aCp2HLqBGA$wdyuuC8SdjTCq1WCiYhpim+bQvNbGIydpWb+/bF01h4','nanaosaki@uni.edu','2005-03-10','Tutor','Bachelor','Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('T005','mina','Mina','Myoi','$argon2id$v=19$m=65536,t=3,p=1$COvewlINkoBLKNjiT0Bitw$81QWSaf/8/euX87Y+YT9wy9VDtlfVj90BE2n69/hh4M','mina@uni.edu','1994-04-09','Tutor','Bachelor','Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$Wy4rwJ6Nr/6UthNKcAeuqw$S5wUcfv5Y5ln+/RYCoZaUwA/629r0p0ZJbUpMkzw6qo',NULL),
('T006','momo','Hirai','Momo','$argon2id$v=19$m=65536,t=3,p=1$lPBOXWsmtFcnpMRPe9+TwQ$H9yoWh0SxCoY9AD7hCcaVCfeXiitLH7EHb15ijs705g','momo123@gmail.com','1996-11-09','Tutor','Master','Active',NULL,NULL,NULL,NULL,NULL,NULL),
('T007','kento','Kento','Yamazaki','$argon2id$v=19$m=65536,t=3,p=1$V0nCbgbAGoOijN7GTviYCA$MVld5AqS8y4v1WIWTloO0rkGWFs+Jeu3x07SyoLNmiM','kento@gmail.com','2000-01-01','Tutor','Bachelor','Active',NULL,NULL,NULL,'What city were you born in?','$argon2id$v=19$m=65536,t=3,p=1$5GCDjZ71SvS1RTjhU28vTg$Jf7CJZgeuQQyGlgu7C7y1tKKurgRCdScYLF55HM2xMA',NULL);
GO

-- Insert Sample Learning Materials
INSERT INTO LearningMaterials (Title, Description, Content, Chapter, TutorID)
VALUES 
    ('Introduction to Programming', 'Basics of programming concepts', 'Programming is...', 1, 'T001'),
    ('Variables and Data Types', 'Understanding variables in C#', 'Variables are...', 1, 'T001'),
    ('Loops and Conditionals', 'Control flow in programming', 'Loops allow you to...', 2, 'T001');
GO

-- Insert Sample Mini Game Questions
INSERT INTO MiniGameQuestions (Question, CorrectAnswer, Explanation, Category, Difficulty)
VALUES 
    ('What is a variable?', 'A named storage location', 'Variables store data values', 'Basics', 1),
    ('What does ''if'' statement do?', 'Checks a condition', 'If statement executes code based on condition', 'Conditionals', 1),
    ('Write a for loop in C#', 'for(int i=0; i<10; i++)', 'This loops from 0 to 9', 'Loops', 2);
GO

-- Insert Sample Forum Posts
INSERT INTO ForumPosts (Title, Content, UserID, Category, IsApproved)
VALUES 
    ('How to start learning programming?', 'What resources do you recommend?', 'S002', 'General', 1),
    ('Need help with loops', 'I don''t understand for loops', 'S003', 'Help', 1);
GO

-- Insert Sample Forum Replies
INSERT INTO ForumReplies (PostID, UserID, Content, IsApproved)
VALUES 
    (1, 'T001', 'Start with the basics in chapter 1', 1),
    (2, 'T001', 'Check the learning materials on loops', 1);
GO

-- Insert Sample User Progress
INSERT INTO UserProgress (UserID, QuestionID, Score, TimeSpent)
VALUES 
    ('S002', 1, 100, 120),
    ('S002', 2, 80, 150),
    ('S003', 1, 90, 100);
GO

-- Insert Sample Self Assessments
INSERT INTO SelfAssessments (UserID, Topic, Score, Feedback)
VALUES 
    ('S002', 'Variables', 85, 'Good understanding of variables'),
    ('S003', 'Loops', 70, 'Need more practice with loops');
GO

-- Insert Sample Tutor Assignments
INSERT INTO TutorAssignments (TutorID, StudentID)
VALUES 
    ('T001', 'S002'),
    ('T001', 'S003');
GO

PRINT 'Sample data inserted successfully.';
GO