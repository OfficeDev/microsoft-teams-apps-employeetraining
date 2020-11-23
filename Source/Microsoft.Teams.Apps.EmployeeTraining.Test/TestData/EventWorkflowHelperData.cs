namespace Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData
{   
    extern alias BetaLib;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using EventType = Microsoft.Teams.Apps.EmployeeTraining.Models.EventType;

    public static class EventWorkflowHelperData
    {
        public static readonly IOptions<BotSettings> botOptions = Options.Create(new BotSettings()
        {
            MicrosoftAppId = "{Application id}",
            MicrosoftAppPassword = "{Application password or secret}",
            AppBaseUri = "https://2db43ef5248b.ngrok.io/",
            EventsPageSize = 50,
        });

        public static readonly IOptions<AzureSettings> azureSettings = Options.Create(new AzureSettings()
        {
            ClientId = "{Application id}",
        });

        public static EventEntity eventEntity;
        public static EventEntity validEventEntity;
        public static List<EventEntity> eventEntities;
        public static Category category;
        public static List<Category> categoryList;
        public static List<Graph.User> graphUsers;
        public static Graph.User graphUser;
        public static List<Graph.DirectoryObject> graphGroupDirectoryObject;
        public static List<Graph.Group> graphGroups;
        public static FormFile fileInfo;
        public static List<TeamsChannelAccount> teamsChannelAccount;
        public static Microsoft.Graph.Event teamEvent;
        public static LnDTeam lndTeam;
        static EventWorkflowHelperData()
        {
            eventEntity = new EventEntity
            {
                EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baz-1234-2345",
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baz-1234",
                Audience = 3,
                CategoryId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02",
                AutoRegisteredAttendees = "",
                CategoryName = "Test_Category",
                CreatedBy = "Jack",
                CreatedOn = new DateTime(2020, 09, 24),
                Description = "Teams Event",
                EndDate = new DateTime(2020, 09, 25),
                EndTime = new DateTime(2020, 09, 25),
                ETag = "",
                GraphEventId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02g",
                IsAutoRegister = false,
                IsRegistrationClosed = false,
                IsRemoved = false,
                MandatoryAttendees = "",
                MaximumNumberOfParticipants = 10,
                MeetingLink = "",
                Name = "Mandaotory Training Event",
                NumberOfOccurrences = 1,
                OptionalAttendees = "",
                Photo = "https://testurl/img.png",
                StartDate = new DateTime(2020, 09, 25),
                StartTime = new DateTime(2020, 09, 25),
                UpdatedBy = "Jack",
                Venue = "",
                SelectedUserOrGroupListJSON = "",
                RegisteredAttendeesCount = 0,
                Type = 0,
                RegisteredAttendees = ""
            };

            validEventEntity = new EventEntity
            {
                EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baz-1234-2345",
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baz-1234",
                Audience = 1,
                CategoryId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02",
                AutoRegisteredAttendees = "",
                CategoryName = "Test_Category",
                CreatedBy = "Jack",
                CreatedOn = DateTime.UtcNow,
                Description = "Teams Event",
                EndDate = DateTime.UtcNow.AddDays(4).Date,
                EndTime = DateTime.UtcNow.AddDays(4).Date,
                ETag = "",
                GraphEventId = "088ddf0d-4deb-4e95-b1f3-907fc4511b02g",
                IsAutoRegister = false,
                IsRegistrationClosed = false,
                IsRemoved = false,
                MandatoryAttendees = "",
                MaximumNumberOfParticipants = 10,
                MeetingLink = "",
                Name = "Mandaotory Training Event",
                NumberOfOccurrences = 1,
                OptionalAttendees = "",
                Photo = "https://www.testurl.com/img.png",
                StartDate = DateTime.UtcNow.AddDays(2).Date,
                StartTime = DateTime.UtcNow.AddDays(2).Date,
                UpdatedBy = "Jack",
                Venue = "",
                SelectedUserOrGroupListJSON = "",
                RegisteredAttendeesCount = 0,
                Type = 2,
                RegisteredAttendees = ""                
            };

            eventEntities = new List<EventEntity>()
            {
                new EventEntity
                {
                    EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-888",
                    CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba",
                    CategoryName = ""
                },
                new EventEntity
                {
                    EventId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999",
                    CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac3ba",
                    CategoryName = ""
                }
            };

            category = new Category
            {
                CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba",
                Name = "Test_Category_1",
                Description = "Description",
                CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
            };

            categoryList = new List<Category>
            {
                new Category
                {
                    CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac1ba",
                    Name = "Test_Category_1",
                    Description = "Description",
                    CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    IsInUse = false,
                },
                new Category
                {
                    CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba",
                    Name = "Test_Category_1",
                    Description = "Description",
                    CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    IsInUse = false,
                },
                new Category
                {
                    CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac3ba",
                    Name = "Test_Category_1",
                    Description = "Description",
                    CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    IsInUse = false,
                }

            };

            teamEvent = new Event
            {
                Subject = "Teams Event",
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = eventEntity.Type == (int)EventType.LiveEvent ?
                        $"{eventEntity.Description}<br/><br/><a href='{eventEntity.MeetingLink}'>{eventEntity.MeetingLink}</a>" :
                        eventEntity.Description,
                },
                Attendees = new List<Attendee>(),
                OnlineMeetingUrl = eventEntity.Type == (int)EventType.LiveEvent ? eventEntity.MeetingLink : null,
                IsReminderOn = true,
                Location = eventEntity.Type == (int)EventType.InPerson ? new Location
                {
                    Address = new PhysicalAddress { Street = eventEntity.Venue },
                }
                    : null,
                AllowNewTimeProposals = false,
                IsOnlineMeeting = true,
                OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness,
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-rrtyy"

            };

            lndTeam = new LnDTeam
            {
                ETag = "",
                PartitionKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999",
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-000",
                RowKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-000"
            };

            graphUser = new Graph.User
            {
                DisplayName = "Jack",
                UserPrincipalName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                Mail = "a@user.com"
            };

            graphUsers = new List<Graph.User>()
            {
                new Graph.User
                {
                    DisplayName = "Jack",
                    UserPrincipalName ="Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                    Mail = "a@user.com"
                },
                new Graph.User
                {
                    DisplayName = "Jack",
                    UserPrincipalName ="Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
                    Mail = "b@user.com"
                },
                new Graph.User
                {
                    DisplayName = "Jack",
                    UserPrincipalName ="Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
                    Mail = "c@user.com"
                }
            };
            
            graphGroups = new List<Graph.Group>()
            {
                new Graph.Group
                {
                    DisplayName = "Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                    Mail = "a@group.com"
                },
                new Graph.Group
                {
                    DisplayName = "Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
                     Mail = "b@group.com"
                },
                new Graph.Group
                {
                    DisplayName = "Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
                     Mail = "c@group.com"
                }
            };

            graphGroupDirectoryObject = new List<Graph.DirectoryObject>()
            {
                new Graph.DirectoryObject
                {

                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001"
                },
                new Graph.DirectoryObject
                {
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002"
                },
                new Graph.DirectoryObject
                {
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003"
                }
            };

            fileInfo = new FormFile(new MemoryStream(), 1, 1, "sample.jpeg", "sample.jpeg");

            teamsChannelAccount = new List<TeamsChannelAccount>()
            {
                new TeamsChannelAccount
                {
                    GivenName="sam",
                    UserPrincipalName="s"
                },
                new TeamsChannelAccount
                {
                    GivenName="jack",
                    UserPrincipalName="j"
                }
            };

            teamEvent = new Event
            {
                Subject = "Teams Event",
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = eventEntity.Type == (int)EventType.LiveEvent ?
                        $"{eventEntity.Description}<br/><br/><a href='{eventEntity.MeetingLink}'>{eventEntity.MeetingLink}</a>" :
                        eventEntity.Description,
                },
                Attendees = new List<Attendee>(),
                OnlineMeetingUrl = eventEntity.Type == (int)EventType.LiveEvent ? eventEntity.MeetingLink : null,
                IsReminderOn = true,
                Location = eventEntity.Type == (int)EventType.InPerson ? new Location
                {
                    Address = new PhysicalAddress { Street = eventEntity.Venue },
                }
                    : null,
                AllowNewTimeProposals = false,
                IsOnlineMeeting = true,
                OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness,
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-rrtyy"

            };

            lndTeam = new LnDTeam
            {
                ETag = "",
                PartitionKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999",
                TeamId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-000",
                RowKey = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-000"
            };

            graphUser = new Graph.User
            {
                DisplayName = "Jack",
                UserPrincipalName = "Jack",
                Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                Mail = "a@user.com"
            };

            graphUsers = new List<Graph.User>()
            {
                new Graph.User
                {
                    DisplayName = "Jack",
                    UserPrincipalName ="Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                    Mail = "a@user.com"
                },
                new Graph.User
                {
                    DisplayName = "Jack",
                    UserPrincipalName ="Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
                    Mail = "b@user.com"
                },
                new Graph.User
                {
                    DisplayName = "Jack",
                    UserPrincipalName ="Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
                    Mail = "c@user.com"
                }
            };

            graphGroups = new List<Graph.Group>()
            {
                new Graph.Group
                {
                    DisplayName = "Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001",
                    Mail = "a@group.com"
                },
                new Graph.Group
                {
                    DisplayName = "Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002",
                     Mail = "b@group.com"
                },
                new Graph.Group
                {
                    DisplayName = "Jack",
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003",
                     Mail = "c@group.com"
                }
            };

            graphGroupDirectoryObject = new List<Graph.DirectoryObject>()
            {
                new Graph.DirectoryObject
                {

                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-001"
                },
                new Graph.DirectoryObject
                {
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-002"
                },
                new Graph.DirectoryObject
                {
                    Id = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba-445567-999-003"
                }
            };

            fileInfo = new FormFile(new MemoryStream(), 1, 1, "sample.jpeg", "sample.jpeg");

            teamsChannelAccount = new List<TeamsChannelAccount>()
            {
                new TeamsChannelAccount
                {
                    GivenName="sam",
                    UserPrincipalName="s"
                },
                new TeamsChannelAccount
                {
                    GivenName="jack",
                    UserPrincipalName="j"
                }
            };
        }
    }
}