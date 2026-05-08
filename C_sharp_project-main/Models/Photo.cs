namespace MyPersonalSpace.Models;

public class Photo
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int AlbumId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.Now;
    public string? Description { get; set; }

    public virtual Album? Album { get; set; }
}
