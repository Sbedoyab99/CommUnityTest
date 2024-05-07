using CommUnity.FrontEnd.Repositories;
using CommUnity.Shared.Entities;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;
using System.Threading.Tasks;

namespace CommUnity.FrontEnd.Pages.ResidentialUnits
{
    public partial class ResidentialUnitsIndex
    {
        public List<ResidentialUnit>? ResidentialUnits { get; set; }

        private readonly int[] pageSizeOptions = { 10, 25, 50, int.MaxValue };
        private MudTable<ResidentialUnit> table = new();
        private int currentRecordsNumber = 10;
        private int totalRecords = 0;

        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;

        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            var ok = await LoadTotalRecords();
            if (ok)
            {
                var state = new TableState { Page = 0, PageSize = currentRecordsNumber };
                await LoadListAsync(state);
            }
        }

        private async Task<bool> LoadTotalRecords()
        {
            string baseUrl = "api/residentialUnit";
            string url;

            url = $"{baseUrl}?page=1&recordsnumber={int.MaxValue}";
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Filter}";
            }
            var responseHttp = await Repository.GetAsync<List<ResidentialUnit>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Title = "Error",
                    Text = message,
                    Icon = SweetAlertIcon.Error
                });
                return false;
            }
            totalRecords = responseHttp.Response!.Count;
            return true;
        }

        private async Task<TableData<ResidentialUnit>> LoadListAsync(TableState state)
        {
            int page = state.Page + 1;
            int pageSize = state.PageSize;

            string baseUrl = "api/residentialUnit";
            string url;

            url = $"{baseUrl}?page={page}&recordsnumber={pageSize}";
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Filter}";
            }

            var responseHttp = await Repository.GetAsync<List<ResidentialUnit>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Title = "Error",
                    Text = message,
                    Icon = SweetAlertIcon.Error
                });
                return new TableData<ResidentialUnit> { Items = new List<ResidentialUnit>(), TotalItems = 0 };
            }

            if(responseHttp.Response == null)
            {
                return new TableData<ResidentialUnit> { Items = new List<ResidentialUnit>(), TotalItems = 0 };
            }
            return new TableData<ResidentialUnit>
            {
                Items = responseHttp.Response,
                TotalItems = totalRecords 
            };
        }

        private async Task SetFilterValue(string value)
        {
            Filter = value;
            await LoadAsync();
            await table.ReloadServerData();
        }

        private async Task DeleteAsync(ResidentialUnit residentialUnit)
        {
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "�Est�s seguro?",
                Text = $"�Est�s seguro de que quieres eliminar la unidad residencial {residentialUnit.Name}?",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
            });
            var confirm = string.IsNullOrEmpty(result.Value);
            if (confirm)
            {
                return;
            }

            var responseHttp = await Repository.DeleteAsync<ResidentialUnit>($"api/residentialUnit/{residentialUnit.Id}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/residentialUnit");
                }
                else
                {
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync(new SweetAlertOptions
                    {
                        Title = "Error",
                        Text = message,
                        Icon = SweetAlertIcon.Error
                    });
                }
                return;
            }
            await LoadAsync();
            await table.ReloadServerData();
            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync("Unidad Resdencial Eliminada", string.Empty, SweetAlertIcon.Success);
        }
    }
}