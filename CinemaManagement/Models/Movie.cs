using System;
using System.Collections.Generic;

namespace CinemaManagement.Models;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public short DurationMinutes { get; set; }

    public string? Genre { get; set; }

    public string? AgeRating { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public string? Language { get; set; }

    public string? PosterUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
