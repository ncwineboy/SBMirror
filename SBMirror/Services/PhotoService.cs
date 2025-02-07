using System.Timers;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using SBMirror.Logic;
using SBMirror.Models;

namespace SBMirror.Services
{
    public class PhotoService : MirrorModuleServiceBase<ConfigPhotos>
    {
        private readonly System.Timers.Timer? _displayTimer;

        List<MediaItem> mediaItems = new List<MediaItem>();
        MediaItem mediaItem = new MediaItem();
        private string[] scopes = { PhotosLibraryService.Scope.PhotoslibraryReadonly };

        public event Action<MediaItem>? PhotoChanged;

        public PhotoService(IHttpClientFactory factory, ILoggerFactory logger) : base(factory, logger, "Photos")
        {
            if (_config != null && _config.IsValid())
            {
                _displayTimer = new System.Timers.Timer(TimeSpan.FromSeconds(_config.displayLengthInSeconds).TotalMilliseconds);
                _displayTimer.Elapsed += async (sender, e) => await DisplayTimerTick(sender, e);
                _displayTimer.Enabled = true;
            }
        }

        public override async Task TimerTick(object? sender, ElapsedEventArgs e)
        {
            mediaItems = await GetPhotos();
        }

        public async Task DisplayTimerTick(object? sender, ElapsedEventArgs e)
        {
            if (mediaItems == null || mediaItems.Count == 0)
            {
                mediaItems = await GetPhotos();
            }
            mediaItem = PickCurrentPhoto();
            PhotoChanged?.Invoke(mediaItem);
        }

        public MediaItem PickCurrentPhoto()
        {
            if (mediaItems == null || mediaItems.Count == 0)
            {
                return new MediaItem();
            }
            var subset = CurrentMonthPhotos();
            var randomIndex = new Random().Next(0, subset.Count - 1);
            var returnval = subset[randomIndex];
            return returnval;
        }

        private List<MediaItem> CurrentMonthPhotos()
        {
            return mediaItems.Where(x => (DateTime.SpecifyKind((DateTime)x.MediaMetadata.CreationTime, DateTimeKind.Utc)).Month == DateTime.UtcNow.Month).ToList();
        }

        public async Task<List<MediaItem>> GetPhotos()
        {
            var returnval = new List<MediaItem>();
            if (_config != null)
            {
                UserCredential credential;
                try
                {
                    using (var stream = new FileStream("client_secret_301924068000-v30jspi0cb8juh876b2t5260pprf7838.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
                    {
                        string credPath = Environment.GetFolderPath(
                        Environment.SpecialFolder.Personal);
                        credPath = Path.Combine(credPath, ".credentials");

                        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true));
                    }
                    var service = new PhotosLibraryService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "SBMirror",
                    });
                    string? pageToken = null;

                    var albumsreq = service.Albums.List();
                    var albums = await albumsreq.ExecuteAsync();
                    if (albums != null)
                    {
                        var album = albums.Albums?.FirstOrDefault(x => x.Title == _config.AlbumName);
                        if (album != null)
                        {
                            do
                            {
                                var request = service.MediaItems.Search(new SearchMediaItemsRequest
                                {
                                    AlbumId = album.Id,
                                    PageSize = 100,
                                    PageToken = pageToken
                                });
                                var response = await request.ExecuteAsync();

                                if (response.MediaItems != null)
                                {
                                    returnval.AddRange(response.MediaItems);
                                }
                                pageToken = response.NextPageToken;
                            } while (!string.IsNullOrEmpty(pageToken));
                        }
                    }
                }
                catch (AggregateException ae)
                {
                    _logger.LogError(ae.InnerException?.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting photos");
                }
            }
            return returnval;
        }
    }
}
