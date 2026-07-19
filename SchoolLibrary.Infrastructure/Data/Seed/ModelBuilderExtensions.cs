using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolLibrary.Infrastructure.Data.Seed
{
    public static class ModelBuilderExtensions
    {
        public static void SeedInitialData(this ModelBuilder modelBuilder)
        {
            SeedGradeLevels(modelBuilder);
            SeedSchoolClasses(modelBuilder);
            SeedSubjects(modelBuilder);
            SeedCategories(modelBuilder);
        }

        private static void SeedGradeLevels(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GradeLevel>().HasData(
                new GradeLevel { Id = 5, Number = 5 },
                new GradeLevel { Id = 6, Number = 6 },
                new GradeLevel { Id = 7, Number = 7 },
                new GradeLevel { Id = 8, Number = 8 },
                new GradeLevel { Id = 9, Number = 9 },
                new GradeLevel { Id = 10, Number = 10 },
                new GradeLevel { Id = 11, Number = 11 },
                new GradeLevel { Id = 12, Number = 12 }
            );
        }

        private static void SeedSchoolClasses(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SchoolClass>().HasData(
                new SchoolClass
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    GradeLevelId = 5,
                    Section = "а"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
                    GradeLevelId = 6,
                    Section = "а"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                    GradeLevelId = 7,
                    Section = "а"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("80000000-0000-0000-0000-000000000001"),
                    GradeLevelId = 8,
                    Section = "а"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("80000000-0000-0000-0000-000000000002"),
                    GradeLevelId = 8,
                    Section = "б"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("80000000-0000-0000-0000-000000000003"),
                    GradeLevelId = 8,
                    Section = "в"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("80000000-0000-0000-0000-000000000004"),
                    GradeLevelId = 8,
                    Section = "г"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000001"),
                    GradeLevelId = 9,
                    Section = "а"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000002"),
                    GradeLevelId = 9,
                    Section = "б"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000003"),
                    GradeLevelId = 9,
                    Section = "в"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000004"),
                    GradeLevelId = 9,
                    Section = "г"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    GradeLevelId = 10,
                    Section = "а"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    GradeLevelId = 10,
                    Section = "б"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                    GradeLevelId = 10,
                    Section = "в"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                    GradeLevelId = 10,
                    Section = "г"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("11000000-0000-0000-0000-000000000001"),
                    GradeLevelId = 11,
                    Section = "а"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("11000000-0000-0000-0000-000000000002"),
                    GradeLevelId = 11,
                    Section = "б"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("11000000-0000-0000-0000-000000000003"),
                    GradeLevelId = 11,
                    Section = "в"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("11000000-0000-0000-0000-000000000004"),
                    GradeLevelId = 11,
                    Section = "г"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("12000000-0000-0000-0000-000000000001"),
                    GradeLevelId = 12,
                    Section = "а"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("12000000-0000-0000-0000-000000000002"),
                    GradeLevelId = 12,
                    Section = "б"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("12000000-0000-0000-0000-000000000003"),
                    GradeLevelId = 12,
                    Section = "в"
                },
                new SchoolClass
                {
                    Id = Guid.Parse("12000000-0000-0000-0000-000000000004"),
                    GradeLevelId = 12,
                    Section = "г"
                }
            );
        }

        private static void SeedSubjects(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>().HasData(
                new Subject
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Математика"
                },
                new Subject
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Информатика"
                },
                new Subject
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Информационни технологии"
                },
                new Subject
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Физика и астрономия"
                },
                new Subject
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Name = "Български език и литература"
                },
                new Subject
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Name = "Английски език"
                },
                new Subject
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Name = "Немски език"
                },
                new Subject
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Name = "Химия и опазване на околната среда"
                },
                new Subject
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    Name = "Биология и здравно образование"
                },
                new Subject
                {
                    Id = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
                    Name = "История и цивилизации"
                },
                new Subject
                {
                    Id = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111"),
                    Name = "География и икономика"
                }
            );
        }

        private static void SeedCategories(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Name = "Теория"
                },
                new Category
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Name = "Задачи"
                },
                new Category
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Name = "Презентации"
                },
                new Category
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Name = "Тестове"
                },
                new Category
                {
                    Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    Name = "Проекти"
                },
                new Category
                {
                    Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    Name = "Работни листове"
                },
                new Category
                {
                    Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                    Name = "Подготовка за изпит"
                }
            );
        }
    }
}
