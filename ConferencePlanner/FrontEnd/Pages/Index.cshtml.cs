using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ConferenceDTO;
using FrontEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FrontEnd.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<IGrouping<DateTimeOffset?, SessionResponse>> Sessions { get; set; }

        public IEnumerable<(int Offset, DayOfWeek? DayofWeek)> DayOffsets { get; set; }

        public int CurrentDayOffset { get; set; }

        public bool IsAdmin { get; set; }

        [TempData]
        public string Message { get; set; }

        public bool ShowMessage => !string.IsNullOrEmpty(Message);

        protected readonly IApiClient _apiClient;
        private readonly IAuthorizationService _authz;

        public IndexModel(IApiClient apiClient, IAuthorizationService authz)
        {
            _apiClient = apiClient;
            _authz = authz;
        }

        public async Task OnGet(int day = 0)
        {
            CurrentDayOffset = day;

            IsAdmin = await _authz.AuthorizeAsync(User, "Admin");

            var sessions = await _apiClient.GetSessionsAsync();

            var startDate = sessions.Min(s => s.StartTime?.Date);
            var endDate = sessions.Max(s => s.EndTime?.Date);

            var numberOfDays = ((endDate - startDate)?.Days) + 1;

            DayOffsets = Enumerable.Range(0, numberOfDays ?? 0)
                .Select(offset => (offset, (startDate?.AddDays(offset))?.DayOfWeek));

            var filterDate = startDate?.AddDays(day);

            Sessions = sessions.Where(s => s.StartTime?.Date == filterDate)
                               .OrderBy(s => s.TrackId)
                               .GroupBy(s => s.StartTime)
                               .OrderBy(g => g.Key);
        }
    }
}
