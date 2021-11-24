using Assignment3;
using GeographyTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PopulateDatabase
{
    public class Program
    {
        public static void Main()
        {
            AppDbContext database = new();
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // Clear the database.
            database.RemoveRange(database.Cinemas);
            database.RemoveRange(database.Movies);
            database.RemoveRange(database.Screenings);
            database.RemoveRange(database.Tickets);


            // Load movies.
            foreach (string line in File.ReadAllLines("SampleMovies.csv"))
            {
                string[] parts = line.Split(',');
                string title = parts[0];
                string releaseDateString = parts[1];
                string runtimeString = parts[2];
                string posterPath = parts[3];

                int releaseYear = int.Parse(releaseDateString.Split('-')[0]);
                int releaseMonth = int.Parse(releaseDateString.Split('-')[1]);
                int releaseDay = int.Parse(releaseDateString.Split('-')[2]);
                var releaseDate = new DateTime(releaseYear, releaseMonth, releaseDay);

                int runtime = int.Parse(runtimeString);

                Movie movie = new()
                {
                    Title = title,
                    ReleaseDate = releaseDate,
                    Runtime = (short)runtime,
                    PosterPath = posterPath
                };
                database.Movies.Add(movie);
                database.SaveChanges();

            }

            // Load cinemas.
            foreach (string line in File.ReadAllLines("SampleCinemasWithPositions.csv"))
            {
                string[] parts = line.Split(',');
                string city = parts[0];
                string name= parts[1];
                double latitude = double.Parse(parts[2]);
                double longitude = double.Parse(parts[3]);

                Cinema cinema = new()
                {
                    City = city,
                    Name = name
                };
                Coordinate coordinate = new()
                {
                    Longitude = longitude,
                    Latitude = latitude
                };
                cinema.Coordinate = coordinate;
                database.Cinemas.Add(cinema);
                database.SaveChanges();
            }

            // Generate random screenings.

            // Get all cinema IDs.
            var cinemaIDs = database.Cinemas.Select(cinema => cinema.ID).ToList();

            // Get all movie IDs.
            var movieIDs = database.Movies.Select(movie => movie.ID).ToList();

            // Create random screenings for each cinema.
            var random = new Random();
            foreach (int cinemaID in cinemaIDs)
            {
                // Choose a random number of screenings.
                int numberOfScreenings = random.Next(2, 6);
                foreach (int n in Enumerable.Range(0, numberOfScreenings)) {
                    // Pick a random movie.
                    int movieID = movieIDs[random.Next(movieIDs.Count)];

                    // Pick a random hour and minute.
                    int hour = random.Next(24);
                    double[] minuteOptions = { 0, 10, 15, 20, 30, 40, 45, 50 };
                    double minute = minuteOptions[random.Next(minuteOptions.Length)];
                    var time = TimeSpan.FromHours(hour) + TimeSpan.FromMinutes(minute);

                    // Insert the screening into the Screenings table.
                    Screening screening = new()
                    {
                        MovieID = movieID,
                        CinemaID = cinemaID,
                        Time = time
                    };
                    database.Screenings.Add(screening);
                    database.SaveChanges();
                }
            }
        }
    }
}
