using CommUnity.FrontEnd.Repositories;
using CommUnity.Shared.Entities;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace CommUnity.FrontEnd.Pages.Apartments
{
    public partial class ApartmentsIndex
    {
        private ResidentialUnit? residentialUnit;
        private List<Apartment>? apartments;

        private int currentPage = 1;
        private int totalPages;
        private string currentRecordsNumber = "10";

        [Parameter] public int ResidentialUnitId { get; set; }
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;

        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string RecordsNumber { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task<bool> LoadResidentialUnitAsync()
        {
            var responseHttp = await Repository.GetAsync<ResidentialUnit>($"/api/residentialunit/{ResidentialUnitId}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                    return false;
                }

                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            residentialUnit = responseHttp.Response;
            return true;
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadAsync(page);
        }

        private async Task LoadAsync(int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(Page))
            {
                page = Convert.ToInt32(Page);
            }
            var ok = await LoadResidentialUnitAsync();
            if (ok)
            {
                ok = await LoadListAsync(page);
                if (ok)
                {
                    if (RecordsNumber != "todos")
                    {
                        await LoadPagesAsync();
                    }
                }
            }
        }

        private async Task<bool> LoadListAsync(int page)
        {
            string baseUrl = $"api/apartments";
            string url;
            if (currentRecordsNumber == "todos")
            {
                url = $"{baseUrl}/all?id={ResidentialUnitId}";
            }
            else
            {
                url = $"{baseUrl}?id={ResidentialUnitId}&page={page}&recordsnumber={currentRecordsNumber}";
                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    url += $"&filter={Filter}";
                }
            }

            var responseHttp = await Repository.GetAsync<List<Apartment>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Title = "Error",
                    Text = message,
                    Icon = SweetAlertIcon.Error
                });
            }
            apartments = responseHttp.Response;
            return true;
        }

        private async Task LoadPagesAsync()
        {
            string baseUrl = $"api/apartments";
            string url;
            if (currentRecordsNumber == "todos")
            {
                return;
            }
            else
            {
                url = $"{baseUrl}/totalpages?id={ResidentialUnitId}&recordsnumber={currentRecordsNumber}";
                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    url += $"&filter={Filter}";
                }
            }

            var responseHttp = await Repository.GetAsync<int>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Title = "Error",
                    Text = message,
                    Icon = SweetAlertIcon.Error
                });
            }
            totalPages = responseHttp.Response;
        }

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadAsync(page);
            await SelectedPageAsync(page);
        }

        private async Task SetFilterValue(string value)
        {
            Filter = value;
            await ApplyFilterAsync();
        }

        private async Task SetRecordsNumber(string value)
        {
            int page = 1;
            RecordsNumber = value;
            currentRecordsNumber = value;
            await LoadAsync(page);
        }

        private async Task DeleteAsync(Apartment apartment)
        {
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "�Est�s seguro?",
                Text = $"�Est�s seguro de que quieres eliminar el apartamento {apartment.Number}?",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
            });
            var confirm = string.IsNullOrEmpty(result.Value);
            if (confirm)
            {
                return;
            }

            var responseHttp = await Repository.DeleteAsync<ResidentialUnit>($"api/apartments/{apartment.Id}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return;
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
            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync("Registro Eliminado", string.Empty, SweetAlertIcon.Success);
        }
    }
}