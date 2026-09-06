-- Demo data only. All timestamp values use UTC, independent of the MySQL server timezone.
-- This script is idempotent: records with existing fixture IDs or customer emails are unchanged.
SET @seeded_at_utc = UTC_TIMESTAMP(6);

INSERT IGNORE INTO businesses (Id, Name, CreatedAtUtc)
VALUES
    (1, 'Rezerv Fitness', @seeded_at_utc),
    (2, 'Studio Flow', @seeded_at_utc);

INSERT IGNORE INTO customers (Id, FirstName, LastName, Email, CreatedAtUtc)
VALUES
    (1, 'Ava', 'Smith', 'ava.smith@example.com', @seeded_at_utc),
    (2, 'Noah', 'Johnson', 'noah.johnson@example.com', @seeded_at_utc),
    (3, 'Mia', 'Williams', 'mia.williams@example.com', @seeded_at_utc),
    (4, 'Liam', 'Brown', 'liam.brown@example.com', @seeded_at_utc),
    (5, 'Emma', 'Jones', 'emma.jones@example.com', @seeded_at_utc),
    (6, 'Oliver', 'Garcia', 'oliver.garcia@example.com', @seeded_at_utc),
    (7, 'Sophia', 'Miller', 'sophia.miller@example.com', @seeded_at_utc),
    (8, 'Ethan', 'Davis', 'ethan.davis@example.com', @seeded_at_utc),
    (9, 'Isabella', 'Wilson', 'isabella.wilson@example.com', @seeded_at_utc),
    (10, 'James', 'Moore', 'james.moore@example.com', @seeded_at_utc);

INSERT IGNORE INTO timetable_schedules
    (Id, BusinessId, ClassName, Instructor, StartTimeUtc, EndTimeUtc, TotalSlots, AvailableSlots, CreatedAtUtc)
VALUES
    (1001, 1, 'Waitlist Test - Single Slot', 'Jordan Lee', DATE_ADD(@seeded_at_utc, INTERVAL 24 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 25 HOUR), 1, 0, @seeded_at_utc),
    (1002, 1, 'Morning Strength', 'Jordan Lee', DATE_ADD(@seeded_at_utc, INTERVAL 26 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 27 HOUR), 8, 7, @seeded_at_utc),
    (1003, 2, 'Vinyasa Flow', 'Maya Chen', DATE_ADD(@seeded_at_utc, INTERVAL 28 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 29 HOUR), 12, 12, @seeded_at_utc),
    (1004, 1, 'HIIT Circuit', 'Alex Morgan', DATE_ADD(@seeded_at_utc, INTERVAL 48 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 49 HOUR), 6, 4, @seeded_at_utc),
    (1005, 2, 'Pilates Core', 'Maya Chen', DATE_ADD(@seeded_at_utc, INTERVAL 50 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 51 HOUR), 10, 10, @seeded_at_utc),
    (1006, 1, 'Evening Boxing', 'Sam Patel', DATE_ADD(@seeded_at_utc, INTERVAL 58 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 59 HOUR), 4, 4, @seeded_at_utc),
    (1007, 2, 'Restorative Yoga', 'Priya Shah', DATE_ADD(@seeded_at_utc, INTERVAL 72 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 73 HOUR), 15, 15, @seeded_at_utc),
    (1008, 1, 'Functional Training', 'Alex Morgan', DATE_ADD(@seeded_at_utc, INTERVAL 75 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 76 HOUR), 20, 20, @seeded_at_utc),
    (1009, 2, 'Barre Basics', 'Priya Shah', DATE_ADD(@seeded_at_utc, INTERVAL 96 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 97 HOUR), 5, 5, @seeded_at_utc),
    (1010, 1, 'Weekend Mobility', 'Sam Patel', DATE_ADD(@seeded_at_utc, INTERVAL 102 HOUR), DATE_ADD(@seeded_at_utc, INTERVAL 103 HOUR), 3, 3, @seeded_at_utc);

-- Keep the full and partially booked availability values correct when the script is rerun.
UPDATE timetable_schedules
SET StartTimeUtc = CASE Id
        WHEN 1001 THEN DATE_ADD(@seeded_at_utc, INTERVAL 24 HOUR)
        WHEN 1002 THEN DATE_ADD(@seeded_at_utc, INTERVAL 26 HOUR)
        WHEN 1003 THEN DATE_ADD(@seeded_at_utc, INTERVAL 28 HOUR)
        WHEN 1004 THEN DATE_ADD(@seeded_at_utc, INTERVAL 48 HOUR)
        WHEN 1005 THEN DATE_ADD(@seeded_at_utc, INTERVAL 50 HOUR)
        WHEN 1006 THEN DATE_ADD(@seeded_at_utc, INTERVAL 58 HOUR)
        WHEN 1007 THEN DATE_ADD(@seeded_at_utc, INTERVAL 72 HOUR)
        WHEN 1008 THEN DATE_ADD(@seeded_at_utc, INTERVAL 75 HOUR)
        WHEN 1009 THEN DATE_ADD(@seeded_at_utc, INTERVAL 96 HOUR)
        WHEN 1010 THEN DATE_ADD(@seeded_at_utc, INTERVAL 102 HOUR)
    END,
    EndTimeUtc = CASE Id
        WHEN 1001 THEN DATE_ADD(@seeded_at_utc, INTERVAL 25 HOUR)
        WHEN 1002 THEN DATE_ADD(@seeded_at_utc, INTERVAL 27 HOUR)
        WHEN 1003 THEN DATE_ADD(@seeded_at_utc, INTERVAL 29 HOUR)
        WHEN 1004 THEN DATE_ADD(@seeded_at_utc, INTERVAL 49 HOUR)
        WHEN 1005 THEN DATE_ADD(@seeded_at_utc, INTERVAL 51 HOUR)
        WHEN 1006 THEN DATE_ADD(@seeded_at_utc, INTERVAL 59 HOUR)
        WHEN 1007 THEN DATE_ADD(@seeded_at_utc, INTERVAL 73 HOUR)
        WHEN 1008 THEN DATE_ADD(@seeded_at_utc, INTERVAL 76 HOUR)
        WHEN 1009 THEN DATE_ADD(@seeded_at_utc, INTERVAL 97 HOUR)
        WHEN 1010 THEN DATE_ADD(@seeded_at_utc, INTERVAL 103 HOUR)
    END,
    AvailableSlots = CASE Id
        WHEN 1001 THEN 0
        WHEN 1002 THEN 7
        WHEN 1004 THEN 4
        ELSE AvailableSlots
    END
WHERE Id BETWEEN 1001 AND 1010;

INSERT IGNORE INTO packages
    (Id, BusinessId, Name, Description, Credits, IsActive, CreatedAtUtc, ExpiresAtUtc)
VALUES
    (2001, 1, 'Fitness 10 Credit Pack', 'Valid demo package for Rezerv Fitness.', 10, 1, @seeded_at_utc, DATE_ADD(@seeded_at_utc, INTERVAL 30 DAY)),
    (2002, 2, 'Studio 8 Credit Pack', 'Valid demo package for Studio Flow.', 8, 1, @seeded_at_utc, DATE_ADD(@seeded_at_utc, INTERVAL 30 DAY)),
    (2003, 1, 'Expired Fitness Pack', 'Expired demo package for eligibility testing.', 3, 1, @seeded_at_utc, DATE_SUB(@seeded_at_utc, INTERVAL 1 DAY));

INSERT IGNORE INTO customer_packages
    (Id, CustomerId, PackageId, TotalCredits, RemainingCredits, CreatedAtUtc)
VALUES
    (3001, 1, 2001, 10, 9, @seeded_at_utc),
    (3002, 2, 2001, 5, 5, @seeded_at_utc),
    (3003, 3, 2001, 5, 5, @seeded_at_utc),
    (3004, 4, 2001, 5, 4, @seeded_at_utc),
    (3005, 5, 2001, 5, 4, @seeded_at_utc),
    (3006, 6, 2001, 5, 4, @seeded_at_utc),
    (3007, 7, 2002, 8, 8, @seeded_at_utc),
    (3008, 8, 2003, 3, 3, @seeded_at_utc),
    (3009, 9, 2002, 8, 8, @seeded_at_utc),
    (3010, 10, 2001, 5, 5, @seeded_at_utc);

-- Schedule 1001 is full. Customer 2 and then customer 3 form a FIFO waitlist.
INSERT IGNORE INTO bookings
    (Id, CustomerId, TimetableScheduleId, ActiveTimetableScheduleId, CustomerPackageId, Status, CancelledAtUtc, CreatedAtUtc)
VALUES
    (4001, 1, 1001, 1001, 3001, 1, NULL, @seeded_at_utc),
    (4002, 2, 1001, 1001, 3002, 2, NULL, DATE_ADD(@seeded_at_utc, INTERVAL 1 SECOND)),
    (4003, 3, 1001, 1001, 3003, 2, NULL, DATE_ADD(@seeded_at_utc, INTERVAL 2 SECOND)),
    (4004, 4, 1002, 1002, 3004, 1, NULL, @seeded_at_utc),
    (4005, 5, 1004, 1004, 3005, 1, NULL, @seeded_at_utc),
    (4006, 6, 1004, 1004, 3006, 1, NULL, @seeded_at_utc);