-- Insert past events and participants for testing

-- Insert past events
INSERT INTO Events (Id, Name, Time, Location, AdditionalInfo) VALUES 
('11111111-1111-1111-1111-111111111111', 'Jõulukontsert 2024', '2024-12-20 19:00:00', 'Tallinna Lauluväljak', 'Traditsiooniline jõulukontsert kõigile'),
('22222222-2222-2222-2222-222222222222', 'Kevadine töötuba', '2025-03-15 10:00:00', 'Tartu Ülikool', 'Praktilised oskused ja teadmised'),
('33333333-3333-3333-3333-333333333333', 'Suvefestival 2024', '2024-07-10 14:30:00', 'Pärnu Rand', 'Muusika, tants ja lõbus meeleolu');

-- Insert participants for past events
-- For Individual participants
INSERT INTO Participants (FirstName, LastName, PersonalIdCode) VALUES 
('Mari', 'Kask', '38903030001'),
('Jaan', 'Tamm', '39012250002'),
('Liis', 'Mets', '48501120003'),
('Peeter', 'Saar', '37108180004'),
('Anna', 'Kivi', '49203040005');

-- For Company participants  
INSERT INTO Participants (LegalName, RegistryCode, NumberOfParticipants) VALUES 
('AS Tehnoloogia', '12345678', 15),
('OÜ Innovatsioon', '87654321', 8),
('MTÜ Kultuur', '11223344', 25);

-- Link participants to past events
-- Jõulukontsert 2024 participants
INSERT INTO EventParticipants (EventId, ParticipantId, PaymentMethodId, AdditionalInfo) VALUES 
('11111111-1111-1111-1111-111111111111', 1, 1, 'Eriline dieet - gluteenivaba'),
('11111111-1111-1111-1111-111111111111', 2, 2, NULL),
('11111111-1111-1111-1111-111111111111', 6, 1, 'Vajame 15 kohta saalis');

-- Kevadine töötuba participants
INSERT INTO EventParticipants (EventId, ParticipantId, PaymentMethodId, AdditionalInfo) VALUES 
('22222222-2222-2222-2222-222222222222', 3, 1, 'Osalen esmakordselt'),
('22222222-2222-2222-2222-222222222222', 4, 3, NULL),
('22222222-2222-2222-2222-222222222222', 7, 2, 'Meeskond 8 inimesega');

-- Suvefestival 2024 participants
INSERT INTO EventParticipants (EventId, ParticipantId, PaymentMethodId, AdditionalInfo) VALUES 
('33333333-3333-3333-3333-333333333333', 5, 1, 'Tulen koos perega'),
('33333333-3333-3333-3333-333333333333', 1, 2, 'Teine kord festivalil'),
('33333333-3333-3333-3333-333333333333', 8, 3, 'Suur grupp - 25 inimest');
