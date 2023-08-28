﻿using MediCase.WebAPI.Entities.Admin;
using MediCase.WebAPI.Entities.Content;
using MediCase.WebAPI.Entities.Moderator;
using System.Data;
using System.Drawing.Printing;

namespace MediCase.WebAPI
{
    public class Seeder
    {
        private readonly MediCaseAdminContext _dbAdminContext;
        private readonly MediCaseModeratorContext _dbModeratorContext;
        private readonly MediCaseContentContext _dbContentContext;

        public Seeder(MediCaseAdminContext dbAdminContext, MediCaseModeratorContext dbModeratorContext, MediCaseContentContext dbContentContext)
        {
            _dbAdminContext = dbAdminContext;
            _dbModeratorContext = dbModeratorContext;
            _dbContentContext = dbContentContext;
        }

        public void Seed()
        {
            _dbAdminContext.Database.EnsureCreated();
            _dbContentContext.Database.EnsureCreated();
            _dbModeratorContext.Database.EnsureCreated();

            if (!_dbContentContext.EntityTypes.Any()) 
            {
                _dbContentContext.EntityTypes.AddRange(
                    new Entities.Content.EntityType { TypeId = 4, TypeValue = "content" },
                    new Entities.Content.EntityType { TypeId = 6, TypeValue = "content_container" },
                    new Entities.Content.EntityType { TypeId = 11, TypeValue = "glossary" },
                    new Entities.Content.EntityType { TypeId = 12, TypeValue = "glossary_entry" },
                    new Entities.Content.EntityType { TypeId = 3, TypeValue = "navigation" },
                    new Entities.Content.EntityType { TypeId = 5, TypeValue = "question" },
                    new Entities.Content.EntityType { TypeId = 15, TypeValue = "question_container" },
                    new Entities.Content.EntityType { TypeId = 13, TypeValue = "test" },
                    new Entities.Content.EntityType { TypeId = 14, TypeValue = "test_question" }
                );

                _dbContentContext.SaveChanges();
            }

            if (!_dbModeratorContext.EntityTypes.Any()) 
            {
                _dbModeratorContext.EntityTypes.AddRange(
                    new Entities.Moderator.EntityType { TypeId = 4, TypeValue = "content" },
                    new Entities.Moderator.EntityType { TypeId = 6, TypeValue = "content_container" },
                    new Entities.Moderator.EntityType { TypeId = 11, TypeValue = "glossary" },
                    new Entities.Moderator.EntityType { TypeId = 12, TypeValue = "glossary_entry" },
                    new Entities.Moderator.EntityType { TypeId = 3, TypeValue = "navigation" },
                    new Entities.Moderator.EntityType { TypeId = 5, TypeValue = "question" },
                    new Entities.Moderator.EntityType { TypeId = 15, TypeValue = "question_container" },
                    new Entities.Moderator.EntityType { TypeId = 13, TypeValue = "test" },
                    new Entities.Moderator.EntityType { TypeId = 14, TypeValue = "test_question" }
                );

                _dbModeratorContext.SaveChanges();
            }
        }
    }
}