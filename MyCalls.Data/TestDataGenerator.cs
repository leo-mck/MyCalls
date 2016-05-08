using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


namespace MyCalls.Data
{
    public class TestDataGenerator
    {
        Random _random = new Random();

        public void InitializeDatabase()
        {
            using (var db = new AppDbContext())
            {
                if (Database.Exists(db.Database.Connection))
                {
                    return;
                }
            }

            var tags = new List<Tag>();
            tags.AddRange(new[]
            {
                new Tag() {Id = 1, Name = "Personal", Color = "#FFD21C"},
                new Tag() {Id = 2, Name = "Family", Color = "#333333"},
                new Tag() {Id = 3, Name = "Client", Color = "#1C71F2"},
                new Tag() {Id = 4, Name = "800Link", Color = "#FF2D55"}
            });

            var callers = new List<Person>();
            for (int i = 0; i < 50; i++)
            {
                callers.Add(new Person()
                {
                    Id = i + 1,
                    Name = Faker.Name.FullName(),
                    Number = $"{Faker.Address.USZipCode().Substring(0, 4)}-{Faker.Address.USZipCode().Substring(0, 4)}",
                    CallerPriority = RandomEnumValue<CallerPriority>(),
                    DateOfBirth = Faker.Date.Birthday(minAge: 10),
                    Tags = new List<Tag>(RandomTags(tags))
                });
            }

            var calls = new List<Call>();
            var ct = callers.Count();

            for (int i = 0; i < 200; i++)
            {
                var start = Faker.Date.PastWithTime();
                var end = start.AddSeconds(_random.Next(10, (int)TimeSpan.FromHours(1).TotalSeconds));

                var idcaller = _random.Next(1, ct);
                var idcallee = _random.Next(1, ct);

                var caller = callers.Find(x => x.Id == idcaller);
                var callee = callers.Find(x => x.Id == idcallee);

                calls.Add(new Call()
                {
                    Caller = caller,
                    Callee = callee,
                    StartedAt = start,
                    EndedAt = end,
                    DurationSeconds = (int)(end - start).TotalSeconds
                });
            }


            using (var ctx = new AppDbContext())
            {
                ctx.Calls.AddRange(calls);
                ctx.SaveChanges();
            }
        }


        private IEnumerable<Tag> RandomTags(IEnumerable<Tag> tags)
        {
            var ct = tags.Count() + 1;

            for (int i = 0; i <= _random.Next(1, ct); i++)
            {
                var tg = _random.Next(1, ct);
                yield return tags.ToArray()[tg - 1];
            }
        }

        private T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(new Random().Next(v.Length));
        }
    }
}