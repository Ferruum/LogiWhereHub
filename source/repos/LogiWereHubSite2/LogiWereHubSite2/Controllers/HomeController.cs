using LogiWereHubSite2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace LogiWereHubSite2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiIp = "192.168.147.207";

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            //string apiip = "192.168.1.77";
        }

        [HttpGet("/AutchPage")]
        public IActionResult AutchPage()
        {
            return View();
        }
        [HttpGet("/EmailConfirmationPage")]
        public IActionResult EmailConfirmationPage()
        {
            return View();
        }
        [HttpGet("/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet("/Team")]
        public IActionResult Team()
        {
            return View();
        }
        [HttpGet("/Index")]
        public async Task<IActionResult> Index()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var response = await client.GetAsync($"http://{_apiIp}:7020/api/Users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userData = JsonConvert.DeserializeObject<UserModel>(content);

                    return View(userData);
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/Account")]
        public async Task<IActionResult> Account()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var response = await client.GetAsync($"http://{_apiIp}:7020/api/Users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userData = JsonConvert.DeserializeObject<UserModel>(content);

                    return View(userData);
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AutchPage")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/token/signin", model);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<SignInResponse>();

                if (responseData != null && !string.IsNullOrEmpty(responseData.Token) && responseData.UserId != 0)
                {
                    HttpContext.Session.SetString("AuthToken", responseData.Token);
                    HttpContext.Session.SetInt32("UserId", responseData.UserId);
                    HttpContext.Session.SetString("Role", responseData.Role);

                    if (responseData.Role == "Admin")
                    {
                        return RedirectToAction("MainPageAutchUser");
                    }
                    else if (responseData.Role == "User")
                    {
                        if (responseData.EmailConfirmed)
                        {
                            return RedirectToAction("MainPageUser");
                        }
                        else
                        {
                            return RedirectToAction("EmailConfirmationPage");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Неизвестная роль пользователя");
                        return View("AutchPage", model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Не удалось получить токен или идентификатор пользователя");
                    return View("AutchPage", model);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
                return View("AutchPage", model);
            }
        }




        [HttpGet("/OrdersAdmin")]
        public async Task<IActionResult> OrdersAdmin()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Orderrs");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ordersData = JsonConvert.DeserializeObject<List<OrderModel>>(content);

                // Запрос для получения данных о типах заказов
                var orderTypesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes");
                var orderTypesContent = await orderTypesResponse.Content.ReadAsStringAsync();
                var orderTypes = JsonConvert.DeserializeObject<List<OrderTypeModel>>(orderTypesContent);

                // Запрос для получения данных о статусах заказов
                var statusesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Status");
                var statusesContent = await statusesResponse.Content.ReadAsStringAsync();
                var statuses = JsonConvert.DeserializeObject<List<StatusModel>>(statusesContent);

                foreach (var order in ordersData)
                {
                    // Заполняем тип заказа текстовым описанием
                    var orderType = orderTypes.FirstOrDefault(o => o.OrderTypeId == order.OrderTypeId);
                    if (orderType != null)
                    {
                        order.OrderType = orderType.OrderType1;
                    }

                    // Заполняем статус заказа текстовым описанием
                    var status = statuses.FirstOrDefault(s => s.StatusId == order.StatusId);
                    if (status != null)
                    {
                        order.OrderStatus = status.OrderStatus;
                    }

                    // Заполняем имя пользователя
                    var userResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Users/{order.UserId}");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var userContent = await userResponse.Content.ReadAsStringAsync();
                        var user = JsonConvert.DeserializeObject<UserModel>(userContent);
                        order.UserName = $"{user.FirstName} {user.LastName}";
                    }
                }

                // Добавляем проверку роли пользователя
                var role = HttpContext.Session.GetString("Role");
                if (role == "Admin")
                {
                    return View("OrdersAdmin", ordersData);
                }
                else if (role == "User")
                {
                    return RedirectToAction("AccessDenied");
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            else
            {
                return RedirectToAction("Error");
            }
        }



        [HttpGet("/AdminOrderTypes")]
        public async Task<IActionResult> AdminOrderTypes()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var orderTypes = JsonConvert.DeserializeObject<List<OrderTypeModel>>(content);

                // Добавляем проверку роли пользователя
                var role = HttpContext.Session.GetString("Role");
                if (role == "Admin")
                {
                    return View(orderTypes);
                }
                else
                {
                    return RedirectToAction("AccessDenied");
                }
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminPosition")]
        public async Task<IActionResult> AdminPosition()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Positions");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var positions = JsonConvert.DeserializeObject<List<PositionModel>>(content);

                return View(positions);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminProductDescription")]
        public async Task<IActionResult> AdminProductDescription()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/ProductDescriptions");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productDescriptions = JsonConvert.DeserializeObject<List<ProductDescriptionModel>>(content);

                return View("AdminProductDescription", productDescriptions);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }



        [HttpGet("/AdminProduct")]
        public async Task<IActionResult> AdminProduct()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Products");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<ProductModel>>(content);

                return View("AdminProduct", products);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminProductsInShipment")]
        public async Task<IActionResult> AdminProductsInShipment()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/ProductsInShipments");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productsInShipment = JsonConvert.DeserializeObject<List<ProductsInShipmentModel>>(content);

                return View("AdminProductsInShipment", productsInShipment);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminReceipt")]
        public async Task<IActionResult> AdminReceipt()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Receipts");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var receipts = JsonConvert.DeserializeObject<List<ReceiptModel>>(content);

                return View("AdminReceipt", receipts);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminShipment")]
        public async Task<IActionResult> AdminShipment()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Shipments");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var shipments = JsonConvert.DeserializeObject<List<ShipmentModel>>(content);

                return View("AdminShipment", shipments);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminStaff")]
        public async Task<IActionResult> AdminStaff()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/staffs");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var staff = JsonConvert.DeserializeObject<List<staffModel>>(content);

                return View("AdminStaff", staff);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminStatus")]
        public async Task<IActionResult> AdminStatus()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Status");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var statusList = JsonConvert.DeserializeObject<List<StatusModel>>(content);

                return View("AdminStatus", statusList);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminUsers")]
        public async Task<IActionResult> AdminUsers()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Users");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userList = JsonConvert.DeserializeObject<List<UserModel>>(content);

                return View("AdminUsers", userList);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminVehicle")]
        public async Task<IActionResult> AdminVehicle()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Vehicles");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var vehicles = JsonConvert.DeserializeObject<List<VehicleModel>>(content);

                return View("AdminVehicle", vehicles);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminVehicleType")]
        public async Task<IActionResult> AdminVehicleType()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/VehicleTypes");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var vehicleTypes = JsonConvert.DeserializeObject<List<VehicleTypeModel>>(content);

                return View("AdminVehicleType", vehicleTypes);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminWarehouseCapacity")]
        public async Task<IActionResult> AdminWarehouseCapacity()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/WarehouseCapacities");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var capacities = JsonConvert.DeserializeObject<List<WarehouseCapacityModel>>(content);

                return View("AdminWarehouseCapacity", capacities);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminWarehouse")]
        public async Task<IActionResult> AdminWarehouse()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Warehouses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var warehouses = JsonConvert.DeserializeObject<List<WarehouseModel>>(content);

                return View("AdminWarehouse", warehouses);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }



        [HttpGet("/OrdersAdmin/Add")]
        public async Task<IActionResult> AddOrder()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка пользователей
            var usersResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Users");
            var usersContent = await usersResponse.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserModel>>(usersContent);

            // Получение списка типов заказов
            var orderTypesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes");
            var orderTypesContent = await orderTypesResponse.Content.ReadAsStringAsync();
            var orderTypes = JsonConvert.DeserializeObject<List<OrderTypeModel>>(orderTypesContent);

            // Получение списка статусов заказов
            var statusesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Status");
            var statusesContent = await statusesResponse.Content.ReadAsStringAsync();
            var statuses = JsonConvert.DeserializeObject<List<StatusModel>>(statusesContent);

            // Передача данных в представление
            ViewBag.Users = users;
            ViewBag.OrderTypes = orderTypes;
            ViewBag.Statuses = statuses;

            return View();
        }

        [HttpPost("/OrdersAdmin/Add")]
        public async Task<IActionResult> AddOrder(OrderModel newOrder)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список заказов для определения максимального Id и генерации нового уникального Id
            var ordersResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Orderrs");
            var ordersContent = await ordersResponse.Content.ReadAsStringAsync();
            var existingOrders = JsonConvert.DeserializeObject<List<OrderModel>>(ordersContent);
            var maxOrderId = existingOrders.Max(o => o.OrderId);
            newOrder.OrderId = maxOrderId + 1; // Генерируем новый уникальный OrderId

            // Отправка запроса на добавление нового заказа
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Orderrs", newOrder);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("OrdersAdmin");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }



        [HttpGet("/AddOrderType/Add")]
        public async Task<IActionResult> AddOrderType()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка типов заказов
            var orderTypesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes");
            var orderTypesContent = await orderTypesResponse.Content.ReadAsStringAsync();
            var orderTypes = JsonConvert.DeserializeObject<List<OrderTypeModel>>(orderTypesContent);

            // Передача данных в представление
            ViewBag.OrderTypes = orderTypes;

            return View();
        }

        [HttpPost("/AddOrderType/Add")]
        public async Task<IActionResult> AddOrderType(OrderTypeModel newOrderType)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список типов заказов для определения максимального Id и генерации нового уникального Id
            var orderTypesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes");
            var orderTypesContent = await orderTypesResponse.Content.ReadAsStringAsync();
            var existingOrderTypes = JsonConvert.DeserializeObject<List<OrderTypeModel>>(orderTypesContent);
            var maxOrderTypeId = existingOrderTypes.Max(o => o.OrderTypeId);
            newOrderType.OrderTypeId = maxOrderTypeId + 1; // Генерируем новый уникальный OrderTypeId

            // Отправка запроса на добавление нового типа заказа
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/OrderTypes", newOrderType);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminOrderTypes");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminPosition/Add")]
        public async Task<IActionResult> AddPosition()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка должностей
            var positionsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Positions");
            var positionsContent = await positionsResponse.Content.ReadAsStringAsync();
            var positions = JsonConvert.DeserializeObject<List<PositionModel>>(positionsContent);

            // Передача данных в представление
            ViewBag.Positions = positions;

            return View();
        }

        [HttpPost("/AdminPosition/Add")]
        public async Task<IActionResult> AddPosition(PositionModel newPosition)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список должностей для определения максимального Id и генерации нового уникального Id
            var positionsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Positions");
            var positionsContent = await positionsResponse.Content.ReadAsStringAsync();
            var existingPositions = JsonConvert.DeserializeObject<List<PositionModel>>(positionsContent);
            var maxPositionId = existingPositions.Max(p => p.PositionId) ?? 0;
            newPosition.PositionId = maxPositionId + 1; // Генерируем новый уникальный PositionId

            // Отправка запроса на добавление новой должности
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Positions", newPosition);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminPosition");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Методы контроллера
        [HttpGet("/AdminProductDescription/Add")]
        public async Task<IActionResult> AddProductDescription()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка описаний продуктов
            var descriptionsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/ProductDescriptions");
            var descriptionsContent = await descriptionsResponse.Content.ReadAsStringAsync();
            var descriptions = JsonConvert.DeserializeObject<List<ProductDescriptionModel>>(descriptionsContent);

            // Передача данных в представление
            ViewBag.Descriptions = descriptions;

            return View();
        }

        [HttpPost("/AdminProductDescription/Add")]
        public async Task<IActionResult> AddProductDescription(ProductDescriptionModel newDescription)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список описаний продуктов для определения максимального Id и генерации нового уникального Id
            var descriptionsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/ProductDescriptions");
            var descriptionsContent = await descriptionsResponse.Content.ReadAsStringAsync();
            var existingDescriptions = JsonConvert.DeserializeObject<List<ProductDescriptionModel>>(descriptionsContent);
            var maxDescriptionId = existingDescriptions.Max(d => d.DescriptionId) ?? 0;
            newDescription.DescriptionId = maxDescriptionId + 1; // Генерируем новый уникальный DescriptionId

            // Отправка запроса на добавление нового описания продукта
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/ProductDescriptions", newDescription);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProductDescription");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Методы контроллера
        [HttpGet("/AdminProduct/Add")]
        public async Task<IActionResult> AddProduct()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка описаний продуктов
            var descriptionsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/ProductDescriptions");
            var descriptionsContent = await descriptionsResponse.Content.ReadAsStringAsync();
            var descriptions = JsonConvert.DeserializeObject<List<ProductDescriptionModel>>(descriptionsContent);

            // Получение списка складов
            var warehousesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Warehouses");
            var warehousesContent = await warehousesResponse.Content.ReadAsStringAsync();
            var warehouses = JsonConvert.DeserializeObject<List<WarehouseModel>>(warehousesContent);

            // Передача данных в представление
            ViewBag.Descriptions = descriptions;
            ViewBag.Warehouses = warehouses;

            return View();
        }

        [HttpPost("/AdminProduct/Add")]
        public async Task<IActionResult> AddProduct(ProductModel newProduct)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список продуктов для определения максимального Id и генерации нового уникального Id
            var productsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Products");
            var productsContent = await productsResponse.Content.ReadAsStringAsync();
            var existingProducts = JsonConvert.DeserializeObject<List<ProductModel>>(productsContent);
            var maxProductId = existingProducts.Max(p => p.ProductId) ?? 0;
            newProduct.ProductId = maxProductId + 1; // Генерируем новый уникальный ProductId

            // Отправка запроса на добавление нового продукта
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Products", newProduct);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProduct");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Добавление продукта в отгрузку
        [HttpGet("/AdminProductsInShipment/Add")]
        public async Task<IActionResult> AddProductsInShipment()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка продуктов
            var productsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Products");
            var productsContent = await productsResponse.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<ProductModel>>(productsContent);

            // Получение списка отгрузок
            var shipmentsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Shipments");
            var shipmentsContent = await shipmentsResponse.Content.ReadAsStringAsync();
            var shipments = JsonConvert.DeserializeObject<List<ShipmentModel>>(shipmentsContent);

            ViewBag.Products = products;
            ViewBag.Shipments = shipments;

            return View();
        }

        [HttpPost("/AdminProductsInShipment/Add")]
        public async Task<IActionResult> AddProductsInShipment(ProductsInShipmentModel productInShipment)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список продуктов в отгрузке для определения максимального ProductInShipmentId и генерации нового уникального Id
            var productsInShipmentResponse = await client.GetAsync($"http://{_apiIp}:7020/api/ProductsInShipments");
            var productsInShipmentContent = await productsInShipmentResponse.Content.ReadAsStringAsync();
            var existingProductsInShipment = JsonConvert.DeserializeObject<List<ProductsInShipmentModel>>(productsInShipmentContent);
            var maxProductInShipmentId = existingProductsInShipment.Max(p => p.ProductInShipmentId) ?? 0;
            productInShipment.ProductInShipmentId = maxProductInShipmentId + 1; // Генерируем новый уникальный ProductInShipmentId

            // Отправка запроса на добавление продукта в отгрузку
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/ProductsInShipments", productInShipment);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProductsInShipment");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }



        // Добавление квитанции
        [HttpGet("/AdminReceipt/Add")]
        public async Task<IActionResult> AddReceipt()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка заказов
            var ordersResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Orderrs");
            var ordersContent = await ordersResponse.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<OrderModel>>(ordersContent);

            ViewBag.Orders = orders;

            return View();
        }

        [HttpPost("/AdminReceipt/Add")]
        public async Task<IActionResult> AddReceipt(ReceiptModel receipt)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список квитанций для определения максимального ReceiptId и генерации нового уникального Id
            var receiptsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Receipts");
            var receiptsContent = await receiptsResponse.Content.ReadAsStringAsync();
            var existingReceipts = JsonConvert.DeserializeObject<List<ReceiptModel>>(receiptsContent);
            var maxReceiptId = existingReceipts.Max(r => r.ReceiptId) ?? 0;
            receipt.ReceiptId = maxReceiptId + 1; // Генерируем новый уникальный ReceiptId

            // Отправка запроса на добавление квитанции
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Receipts", receipt);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminReceipt");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Добавление отгрузки
        [HttpGet("/AdminShipment/Add")]
        public async Task<IActionResult> AddShipment()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка заказов
            var ordersResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Orderrs");
            var ordersContent = await ordersResponse.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<OrderModel>>(ordersContent);

            ViewBag.Orders = orders;

            // Получение списка транспортных средств
            var vehiclesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Vehicles");
            var vehiclesContent = await vehiclesResponse.Content.ReadAsStringAsync();
            var vehicles = JsonConvert.DeserializeObject<List<VehicleModel>>(vehiclesContent);

            ViewBag.Vehicles = vehicles;

            // Получение списка сотрудников
            var staffResponse = await client.GetAsync($"http://{_apiIp}:7020/api/staffs");
            var staffContent = await staffResponse.Content.ReadAsStringAsync();
            var staff = JsonConvert.DeserializeObject<List<staffModel>>(staffContent);

            ViewBag.Staff = staff;

            return View();
        }

        [HttpPost("/AdminShipment/Add")]
        public async Task<IActionResult> AddShipment(ShipmentModel shipment)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список отгрузок для определения максимального ShipmentId и генерации нового уникального Id
            var shipmentsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Shipments");
            var shipmentsContent = await shipmentsResponse.Content.ReadAsStringAsync();
            var existingShipments = JsonConvert.DeserializeObject<List<ShipmentModel>>(shipmentsContent);
            var maxShipmentId = existingShipments.Max(s => s.ShipmentId) ?? 0;
            shipment.ShipmentId = maxShipmentId + 1; // Генерируем новый уникальный ShipmentId

            // Отправка запроса на добавление отгрузки
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Shipments", shipment);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminShipment");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Добавление сотрудника
        [HttpGet("/AdminStaff/Add")]
        public async Task<IActionResult> AddStaff()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка складов
            var warehousesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Warehouses");
            var warehousesContent = await warehousesResponse.Content.ReadAsStringAsync();
            var warehouses = JsonConvert.DeserializeObject<List<WarehouseModel>>(warehousesContent);

            ViewBag.Warehouses = warehouses;

            // Получение списка должностей
            var positionsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Positions");
            var positionsContent = await positionsResponse.Content.ReadAsStringAsync();
            var positions = JsonConvert.DeserializeObject<List<PositionModel>>(positionsContent);

            ViewBag.Positions = positions;

            // Получение списка пользователей
            var usersResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Users");
            var usersContent = await usersResponse.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserModel>>(usersContent);

            ViewBag.Users = users;

            return View();
        }


        [HttpPost("/AdminStaff/Add")]
        public async Task<IActionResult> AddStaff(staffModel staff)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список сотрудников для определения максимального StaffId и генерации нового уникального Id
            var staffResponse = await client.GetAsync($"http://{_apiIp}:7020/api/staffs");
            var staffContent = await staffResponse.Content.ReadAsStringAsync();
            var existingStaff = JsonConvert.DeserializeObject<List<staffModel>>(staffContent);
            var maxStaffId = existingStaff.Max(s => s.StaffId) ?? 0;
            staff.StaffId = maxStaffId + 1; // Генерируем новый уникальный StaffId

            // Отправка запроса на добавление сотрудника
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/staffs", staff);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminStaff");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Добавление статуса
        [HttpGet("/AdminStatus/Add")]
        public async Task<IActionResult> AddStatus()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            return View();
        }

        [HttpPost("/AdminStatus/Add")]
        public async Task<IActionResult> AddStatus(StatusModel status)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список статусов для определения максимального StatusId и генерации нового уникального Id
            var statusResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Status");
            var statusContent = await statusResponse.Content.ReadAsStringAsync();
            var existingStatuses = JsonConvert.DeserializeObject<List<StatusModel>>(statusContent);
            var maxStatusId = existingStatuses.Max(s => s.StatusId) ?? 0;
            status.StatusId = maxStatusId + 1; // Генерируем новый уникальный StatusId

            // Отправка запроса на добавление статуса
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Status", status);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminStatus");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Добавление транспортного средства
        [HttpGet("/AdminVehicle/Add")]
        public async Task<IActionResult> AddVehicle()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение списка типов транспортных средств
            var vehicleTypesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/VehicleTypes");
            var vehicleTypesContent = await vehicleTypesResponse.Content.ReadAsStringAsync();
            var vehicleTypes = JsonConvert.DeserializeObject<List<VehicleTypeModel>>(vehicleTypesContent);

            ViewBag.VehicleTypes = vehicleTypes;

            return View();
        }

        [HttpPost("/AdminVehicle/Add")]
        public async Task<IActionResult> AddVehicle(VehicleModel vehicle)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список транспортных средств для определения максимального VehicleId и генерации нового уникального Id
            var vehiclesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Vehicles");
            var vehiclesContent = await vehiclesResponse.Content.ReadAsStringAsync();
            var existingVehicles = JsonConvert.DeserializeObject<List<VehicleModel>>(vehiclesContent);
            var maxVehicleId = existingVehicles.Max(v => v.VehicleId) ?? 0;
            vehicle.VehicleId = maxVehicleId + 1; // Генерируем новый уникальный VehicleId

            // Отправка запроса на добавление транспортного средства
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Vehicles", vehicle);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminVehicle");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Добавление типа транспортного средства
        [HttpGet("/AdminVehicleType/Add")]
        public async Task<IActionResult> AddVehicleType()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            return View();
        }

        [HttpPost("/AdminVehicleType/Add")]
        public async Task<IActionResult> AddVehicleType(VehicleTypeModel vehicleType)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список типов транспортных средств для определения максимального VehicleTypeId и генерации нового уникального Id
            var vehicleTypesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/VehicleTypes");
            var vehicleTypesContent = await vehicleTypesResponse.Content.ReadAsStringAsync();
            var existingVehicleTypes = JsonConvert.DeserializeObject<List<VehicleTypeModel>>(vehicleTypesContent);
            var maxVehicleTypeId = existingVehicleTypes.Max(v => v.VehicleTypeId) ?? 0;
            vehicleType.VehicleTypeId = maxVehicleTypeId + 1; // Генерируем новый уникальный VehicleTypeId

            // Отправка запроса на добавление типа транспортного средства
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/VehicleTypes", vehicleType);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminVehicleType");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        // Добавление информации о вместимости склада
        [HttpGet("/AdminWarehouseCapacity/Add")]
        public async Task<IActionResult> AddWarehouseCapacity()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            return View();
        }

        [HttpPost("/AdminWarehouseCapacity/Add")]
        public async Task<IActionResult> AddWarehouseCapacity(WarehouseCapacityModel warehouseCapacity)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список информации о вместимости складов для определения максимального CapacityId и генерации нового уникального Id
            var capacityResponse = await client.GetAsync($"http://{_apiIp}:7020/api/WarehouseCapacities");
            var capacityContent = await capacityResponse.Content.ReadAsStringAsync();
            var existingCapacities = JsonConvert.DeserializeObject<List<WarehouseCapacityModel>>(capacityContent);
            var maxCapacityId = existingCapacities.Max(c => c.CapacityId) ?? 0;
            warehouseCapacity.CapacityId = maxCapacityId + 1; // Генерируем новый уникальный CapacityId

            // Отправка запроса на добавление информации о вместимости склада
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/WarehouseCapacities", warehouseCapacity);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminWarehouseCapacity");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminWarehouse/Add")]
        public async Task<IActionResult> AddWarehouse()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список вместимости складов
            var capacitiesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/WarehouseCapacities");
            var capacitiesContent = await capacitiesResponse.Content.ReadAsStringAsync();
            var capacities = JsonConvert.DeserializeObject<List<WarehouseCapacityModel>>(capacitiesContent);

            ViewBag.Capacities = capacities;

            return View();
        }


        [HttpPost("/AdminWarehouse/Add")]
        public async Task<IActionResult> AddWarehouse(WarehouseModel warehouse)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список складов для определения максимального WarehouseId и генерации нового уникального Id
            var warehousesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Warehouses");
            var warehousesContent = await warehousesResponse.Content.ReadAsStringAsync();
            var existingWarehouses = JsonConvert.DeserializeObject<List<WarehouseModel>>(warehousesContent);
            var maxWarehouseId = existingWarehouses.Max(w => w.WarehouseId) ?? 0;
            warehouse.WarehouseId = maxWarehouseId + 1; // Генерируем новый уникальный WarehouseId

            // Отправка запроса на добавление информации о складе
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Warehouses", warehouse);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminWarehouse");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }



        [HttpPost("/OrdersAdmin/Delete")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление заказа с заданным OrderId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Orderrs/{orderId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("OrdersAdmin");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminOrderTypes/Delete")]
        public async Task<IActionResult> DeleteOrderType(int orderTypeId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление типа заказа с заданным OrderTypeId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/OrderTypes/{orderTypeId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminOrderTypes");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminPosition/Delete")]
        public async Task<IActionResult> DeletePosition(int positionId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление типа заказа с заданным OrderTypeId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Positions/{positionId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminPosition");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminProductDescription/Delete")]
        public async Task<IActionResult> DeleteProductDescription(int descriptionId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление описания продукта с заданным DescriptionId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/ProductDescriptions/{descriptionId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProductDescription");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminProduct/Delete")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление продукта с заданным ProductId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Products/{productId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProduct");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminProductsInShipment/Delete")]
        public async Task<IActionResult> DeleteProductInShipment(int productInShipmentId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление продукта в отгрузке с заданным ProductInShipmentId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/ProductsInShipments/{productInShipmentId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProductsInShipment");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminReceipt/Delete")]
        public async Task<IActionResult> DeleteReceipt(int receiptId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление квитанции с заданным ReceiptId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Receipts/{receiptId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminReceipt");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminShipment/Delete")]
        public async Task<IActionResult> DeleteShipment(int shipmentId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление отгрузки с заданным ShipmentId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Shipments/{shipmentId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminShipment");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminStaff/Delete")]
        public async Task<IActionResult> DeleteStaff(int staffId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление сотрудника с заданным StaffId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Staffs/{staffId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminStaff");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminStatus/Delete")]
        public async Task<IActionResult> DeleteStatus(int statusId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление статуса с заданным StatusId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Status/{statusId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminStatus");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }



        [HttpPost("/AdminVehicle/Delete")]
        public async Task<IActionResult> DeleteVehicle(int vehicleId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление транспортного средства с заданным VehicleId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Vehicles/{vehicleId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminVehicle");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminVehicleType/Delete")]
        public async Task<IActionResult> DeleteVehicleType(int vehicleTypeId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление типа транспортного средства с заданным VehicleTypeId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/VehicleTypes/{vehicleTypeId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminVehicleType");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminWarehouseCapacity/Delete")]
        public async Task<IActionResult> DeleteWarehouseCapacity(int capacityId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление емкости склада с заданным CapacityId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/WarehouseCapacities/{capacityId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminWarehouseCapacity");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/AdminWarehouse/Delete")]
        public async Task<IActionResult> DeleteWarehouse(int warehouseId)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на удаление склада с заданным WarehouseId
            var response = await client.DeleteAsync($"http://{_apiIp}:7020/api/Warehouses/{warehouseId}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminWarehouse");
            }
            else
            {
                // Обработка ошибки, если удаление не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/OrdersAdmin/Edit/{id}")]
        public async Task<IActionResult> EditOrder(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение данных о заказе для редактирования
            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Orderrs/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var order = JsonConvert.DeserializeObject<OrderModel>(content);

                // Получение списка пользователей
                var usersResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Users");
                var usersContent = await usersResponse.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserModel>>(usersContent);

                // Получение списка типов заказов
                var orderTypesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes");
                var orderTypesContent = await orderTypesResponse.Content.ReadAsStringAsync();
                var orderTypes = JsonConvert.DeserializeObject<List<OrderTypeModel>>(orderTypesContent);

                // Получение списка статусов заказов
                var statusesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Status");
                var statusesContent = await statusesResponse.Content.ReadAsStringAsync();
                var statuses = JsonConvert.DeserializeObject<List<StatusModel>>(statusesContent);

                // Передача данных в представление
                ViewBag.Users = users;
                ViewBag.OrderTypes = orderTypes;
                ViewBag.Statuses = statuses;

                return View(order);
            }
            else
            {
                // Обработка ошибки, если данные о заказе не удалось загрузить
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/OrdersAdmin/Edit/{id}")]
        public async Task<IActionResult> EditOrder(int id, OrderModel updatedOrder)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на редактирование заказа
            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Orderrs/{id}", updatedOrder);

            if (response.IsSuccessStatusCode)
            {
                // Перенаправление на страницу OrdersAdmin после успешного редактирования
                return RedirectToAction("OrdersAdmin");
            }
            else
            {
                // Обработка ошибки, если редактирование не удалось
                return RedirectToAction("Error");
            }
        }



        [HttpGet("/AdminOrderTypes/Edit/{id}")]
        public async Task<IActionResult> EditOrderType(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение данных о типе заказа для редактирования
            var response = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var orderType = JsonConvert.DeserializeObject<OrderTypeModel>(content);

                return View(orderType);
            }
            else
            {
                // Обработка ошибки, если данные о типе заказа не удалось загрузить
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminOrderTypes/Edit/{id}")]
        public async Task<IActionResult> EditOrderType(int id, OrderTypeModel updatedOrderType)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на редактирование типа заказа
            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/OrderTypes/{id}", updatedOrderType);

            if (response.IsSuccessStatusCode)
            {
                // Перенаправление на страницу AdminOrderTypes после успешного редактирования
                return RedirectToAction("AdminOrderTypes");
            }
            else
            {
                // Обработка ошибки, если редактирование не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AdminPosition/Edit/{id}")]
        public async Task<IActionResult> EditPosition(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("LoginPage", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Positions/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var position = JsonConvert.DeserializeObject<PositionModel>(content);

                return View(position);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost("/AdminPosition/Edit/{id}")]
        public async Task<IActionResult> EditPosition(int id, PositionModel updatedPosition)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("LoginPage", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Positions/{id}", updatedPosition);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminPosition");
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpGet("/AdminProductDescription/Edit/{id}")]
        public async Task<IActionResult> EditProductDescription(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("LoginPage", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/ProductDescriptions/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productDescription = JsonConvert.DeserializeObject<ProductDescriptionModel>(content);

                return View(productDescription);
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost("/AdminProductDescription/Edit/{id}")]
        public async Task<IActionResult> EditProductDescription(int id, ProductDescriptionModel updatedProductDescription)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("LoginPage", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/ProductDescriptions/{id}", updatedProductDescription);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProductDescription");
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpGet("/AdminProduct/Edit/{id}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("LoginPage", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение продукта для редактирования
            var productResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Products/{id}");
            if (!productResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Error", "Home");
            }
            var productContent = await productResponse.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<ProductModel>(productContent);

            // Предполагаем, что API имеет маршруты для получения списков складов и описаний
            var warehousesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Warehouses");
            var descriptionsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/ProductDescriptions");

            if (warehousesResponse.IsSuccessStatusCode && descriptionsResponse.IsSuccessStatusCode)
            {
                var warehousesContent = await warehousesResponse.Content.ReadAsStringAsync();
                var descriptionsContent = await descriptionsResponse.Content.ReadAsStringAsync();

                var warehouses = JsonConvert.DeserializeObject<List<WarehouseModel>>(warehousesContent);
                var descriptions = JsonConvert.DeserializeObject<List<ProductDescriptionModel>>(descriptionsContent);

                // Использование ViewBag для передачи данных в представление
                ViewBag.Warehouses = new SelectList(warehouses, "WarehouseId", "Name");
                ViewBag.Descriptions = new SelectList(descriptions, "DescriptionId", "Name");
            }

            return View(product);
        }


        [HttpPost("/AdminProduct/Edit/{id}")]
        public async Task<IActionResult> EditProduct(int id, ProductModel updatedProduct)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("LoginPage", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Products/{id}", updatedProduct);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProduct");
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpGet("/AdminProductsInShipment/Edit/{id}")]
        public async Task<IActionResult> EditProductInShipment(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("LoginPage", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var productInShipmentResponse = await client.GetAsync($"http://{_apiIp}:7020/api/ProductsInShipments/{id}");
            if (!productInShipmentResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Error", "Home");
            }
            var productInShipmentContent = await productInShipmentResponse.Content.ReadAsStringAsync();
            var productInShipment = JsonConvert.DeserializeObject<ProductsInShipmentModel>(productInShipmentContent);

            var shipmentsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Shipments");
            var productsResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Products");

            if (shipmentsResponse.IsSuccessStatusCode && productsResponse.IsSuccessStatusCode)
            {
                var shipmentsContent = await shipmentsResponse.Content.ReadAsStringAsync();
                var productsContent = await productsResponse.Content.ReadAsStringAsync();

                var shipments = JsonConvert.DeserializeObject<List<ShipmentModel>>(shipmentsContent);
                var products = JsonConvert.DeserializeObject<List<ProductModel>>(productsContent);

                ViewBag.Shipments = new SelectList(shipments, "ShipmentId", "ShipmentId"); // Можно добавить дополнительные поля для отображения
                ViewBag.Products = new SelectList(products, "ProductId", "Name");
            }

            return View(productInShipment);
        }

        [HttpPost("/AdminProductsInShipment/Edit/{id}")]
        public async Task<IActionResult> EditProductInShipment(int id, ProductsInShipmentModel updatedProductInShipment)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("LoginPage", "Auth");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/ProductsInShipments/{id}", updatedProductInShipment);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminProductsInShipment");
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }


        // Метод для редактирования квитанции
        [HttpGet("/AdminReceipt/Edit/{id}")]
        public async Task<IActionResult> EditReceipt(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Receipts/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var receipt = JsonConvert.DeserializeObject<ReceiptModel>(content);

                return View(receipt);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminReceipt/Edit/{id}")]
        public async Task<IActionResult> EditReceipt(int id, ReceiptModel updatedReceipt)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Receipts/{id}", updatedReceipt);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminReceipt");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        // Метод для редактирования отправления
        [HttpGet("/AdminShipment/Edit/{id}")]
        public async Task<IActionResult> EditShipment(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Shipments/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var shipment = JsonConvert.DeserializeObject<ShipmentModel>(content);

                return View(shipment);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminShipment/Edit/{id}")]
        public async Task<IActionResult> EditShipment(int id, ShipmentModel updatedShipment)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Shipments/{id}", updatedShipment);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminShipment");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        // Метод для редактирования сотрудника
        [HttpGet("/AdminStaff/Edit/{id}")]
        public async Task<IActionResult> EditStaff(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/staffs/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var staff = JsonConvert.DeserializeObject<staffModel>(content);

                return View(staff);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminStaff/Edit/{id}")]
        public async Task<IActionResult> EditStaff(int id, staffModel updatedStaff)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/staffs/{id}", updatedStaff);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminStaff");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        // Метод для редактирования статуса заказа
        [HttpGet("/AdminStatus/Edit/{id}")]
        public async Task<IActionResult> EditStatus(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Status/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var status = JsonConvert.DeserializeObject<StatusModel>(content);

                return View(status);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminStatus/Edit/{id}")]
        public async Task<IActionResult> EditStatus(int id, StatusModel updatedStatus)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Status/{id}", updatedStatus);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminStatus");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        // Метод для редактирования информации о транспортном средстве
        [HttpGet("/AdminVehicle/Edit/{id}")]
        public async Task<IActionResult> EditVehicle(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Vehicles/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var vehicle = JsonConvert.DeserializeObject<VehicleModel>(content);

                return View(vehicle);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminVehicle/Edit/{id}")]
        public async Task<IActionResult> EditVehicle(int id, VehicleModel updatedVehicle)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Vehicles/{id}", updatedVehicle);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminVehicle");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        // Метод для редактирования типа транспортного средства
        [HttpGet("/AdminVehicleType/Edit/{id}")]
        public async Task<IActionResult> EditVehicleType(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/VehicleTypes/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var vehicleType = JsonConvert.DeserializeObject<VehicleTypeModel>(content);

                return View(vehicleType);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminVehicleType/Edit/{id}")]
        public async Task<IActionResult> EditVehicleType(int id, VehicleTypeModel updatedVehicleType)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/VehicleTypes/{id}", updatedVehicleType);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminVehicleType");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        // Метод для редактирования вместимости склада
        [HttpGet("/AdminWarehouseCapacity/Edit/{id}")]
        public async Task<IActionResult> EditWarehouseCapacity(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/WarehouseCapacities/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var capacity = JsonConvert.DeserializeObject<WarehouseCapacityModel>(content);

                return View(capacity);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminWarehouseCapacity/Edit/{id}")]
        public async Task<IActionResult> EditWarehouseCapacity(int id, WarehouseCapacityModel updatedCapacity)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/WarehouseCapacities/{id}", updatedCapacity);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminWarehouseCapacity");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }


        // Метод для редактирования информации о складе
        [HttpGet("/AdminWarehouse/Edit/{id}")]
        public async Task<IActionResult> EditWarehouse(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Warehouses/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var warehouse = JsonConvert.DeserializeObject<WarehouseModel>(content);

                return View(warehouse);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/AdminWarehouse/Edit/{id}")]
        public async Task<IActionResult> EditWarehouse(int id, WarehouseModel updatedWarehouse)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Warehouses/{id}", updatedWarehouse);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminWarehouse");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet("/")]
        public IActionResult MainPage()
        {
            return View();
        }
        [HttpGet("/Auto")]
        public IActionResult Auto()
        {
            return View();
        }
        [HttpGet("/Warehouse")]
        public IActionResult Warehouse()
        {
            return View();
        }
        [HttpGet("/Service")]
        public IActionResult Service()
        {
            return View();
        }
        [HttpGet("/MainPageAutchUser")]
        public IActionResult MainPageAutchUser()
        {
            return View();
        }
        [HttpGet("/MainPageUser")]
        public IActionResult MainPageUser()
        {
            return View();
        }
        [HttpGet("/RegiserPage")]
        public IActionResult RegiserPage()
        {
            return View();
        }

        [HttpPost("/RegiserPage")]
        public async Task<IActionResult> Register(UserModel model)
        {
            var client = _httpClientFactory.CreateClient();

            // Отправляем запрос на регистрацию на ваше API
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Token/register", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CheckYourEmail");
            }
            else
            {
                // Если запрос на регистрацию не удался, отображаем сообщение об ошибке
                ModelState.AddModelError(string.Empty, "Не удалось зарегистрироваться. Пожалуйста, попробуйте снова.");
                return View("RegiserPage", model);
            }
        }
        [HttpGet("/CheckYourEmail")]
        public IActionResult CheckYourEmail()
        {
            return View();
        }

        [HttpGet("/Account/Edit/{id}")]
        public async Task<IActionResult> EditUser(int id)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение данных о пользователе для редактирования
            var response = await client.GetAsync($"http://{_apiIp}:7020/api/Users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(content);

                return View(user);
            }
            else
            {
                // Обработка ошибки, если данные о пользователе не удалось загрузить
                return RedirectToAction("Error");
            }
        }

        [HttpPost("/Account/Edit/{id}")]
        public async Task<IActionResult> EditUser(int id, UserModel updatedUser)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Отправка запроса на редактирование пользователя
            var response = await client.PutAsJsonAsync($"http://{_apiIp}:7020/api/Users/{id}", updatedUser);

            if (response.IsSuccessStatusCode)
            {
                // Перенаправление на страницу с профилем пользователя после успешного редактирования
                return RedirectToAction("Account");
            }
            else
            {
                // Обработка ошибки, если редактирование не удалось
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/UserProducts")]
        public async Task<IActionResult> UserProducts()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("AuthPage");
            }

            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                var response = await client.GetAsync($"http://{_apiIp}:7020/UserProducts/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userProducts = JsonConvert.DeserializeObject<List<ProductModel>>(content);

                    // Получаем дополнительные данные для каждого продукта
                    foreach (var product in userProducts)
                    {
                        // Запрос для получения данных о продукте по его DescriptionId
                        var descriptionResponse = await client.GetAsync($"http://{_apiIp}:7020/api/ProductDescriptions/{product.DescriptionId}");
                        if (descriptionResponse.IsSuccessStatusCode)
                        {
                            var descriptionContent = await descriptionResponse.Content.ReadAsStringAsync();
                            var description = JsonConvert.DeserializeObject<ProductDescriptionModel>(descriptionContent);
                            product.Name = description.Name; // Заполняем название продукта из связанной модели
                        }

                        // Запрос для получения данных о складе по его WarehouseId
                        var warehouseResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Warehouses/{product.WarehouseId}");
                        if (warehouseResponse.IsSuccessStatusCode)
                        {
                            var warehouseContent = await warehouseResponse.Content.ReadAsStringAsync();
                            var warehouse = JsonConvert.DeserializeObject<WarehouseModel>(warehouseContent);
                            product.NameWarehouse = warehouse.Name; // Заполняем название склада из связанной модели
                            product.Address = warehouse.Address; // Заполняем адрес склада из связанной модели
                        }
                    }

                    return View(userProducts);
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }


        [HttpPost("/Logout")]
        public IActionResult Logout()
        {
            // Очищаем сессию пользователя
            HttpContext.Session.Clear();
            // Перенаправляем пользователя на главную страницу
            return RedirectToAction("MainPage");
        }


        [HttpGet("/OrdersUser")]
        public async Task<IActionResult> OrdersUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("AuthPage");
            }

            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AuthPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                var response = await client.GetAsync($"http://{_apiIp}:7020/UserOrders/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userOrders = JsonConvert.DeserializeObject<List<OrderModel>>(content);

                    // Получаем дополнительные данные для каждого заказа
                    foreach (var order in userOrders)
                    {
                        // Запрос для получения данных о типе заказа по его OrderTypeId
                        var orderTypeResponse = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes/{order.OrderTypeId}");
                        if (orderTypeResponse.IsSuccessStatusCode)
                        {
                            var orderTypeContent = await orderTypeResponse.Content.ReadAsStringAsync();
                            var orderType = JsonConvert.DeserializeObject<OrderTypeModel>(orderTypeContent);
                            order.OrderType = orderType.OrderType1; // Заполняем тип заказа из связанной модели
                        }

                        // Запрос для получения данных о статусе заказа по его StatusId
                        var statusResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Status/{order.StatusId}");
                        if (statusResponse.IsSuccessStatusCode)
                        {
                            var statusContent = await statusResponse.Content.ReadAsStringAsync();
                            var status = JsonConvert.DeserializeObject<StatusModel>(statusContent);
                            order.OrderStatus = status.OrderStatus; // Заполняем статус заказа из связанной модели
                        }

                        // Запрос для получения данных о пользователе по его UserId
                        var userResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Users/{order.UserId}");
                        if (userResponse.IsSuccessStatusCode)
                        {
                            var userContent = await userResponse.Content.ReadAsStringAsync();
                            var user = JsonConvert.DeserializeObject<UserModel>(userContent);
                            order.UserName = $"{user.FirstName} {user.LastName}"; // Заполняем имя пользователя
                        }
                    }

                    return View(userOrders);
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }


        [HttpGet("/AddOrderUser/Add")]
        public async Task<IActionResult> AddOrderUser()
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получение текущего пользователя
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("AuthPage");
            }

            // Получение списка типов заказов
            var orderTypesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/OrderTypes");
            var orderTypesContent = await orderTypesResponse.Content.ReadAsStringAsync();
            var orderTypes = JsonConvert.DeserializeObject<List<OrderTypeModel>>(orderTypesContent);

            // Получение списка статусов заказов
            var statusesResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Status");
            var statusesContent = await statusesResponse.Content.ReadAsStringAsync();
            var statuses = JsonConvert.DeserializeObject<List<StatusModel>>(statusesContent);

            // Передача данных в представление
            ViewBag.OrderTypes = orderTypes;
            ViewBag.Statuses = statuses;

            var newOrder = new OrderModel
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now, 
                OrderTypeId = 1,
                StatusId = 1
            };

            return View(newOrder);
        }


        [HttpPost("/AddOrderUser/Add")]
        public async Task<IActionResult> AddOrderUser(OrderModel newOrder)
        {
            var authToken = HttpContext.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToAction("AutchPage");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            // Получаем список заказов для определения максимального Id и генерации нового уникального Id
            var ordersResponse = await client.GetAsync($"http://{_apiIp}:7020/api/Orderrs");
            var ordersContent = await ordersResponse.Content.ReadAsStringAsync();
            var existingOrders = JsonConvert.DeserializeObject<List<OrderModel>>(ordersContent);
            var maxOrderId = existingOrders.Max(o => o.OrderId);
            newOrder.OrderId = maxOrderId + 1; // Генерируем новый уникальный OrderId

            // Отправка запроса на добавление нового заказа
            var response = await client.PostAsJsonAsync($"http://{_apiIp}:7020/api/Orderrs", newOrder);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("OrdersUser");
            }
            else
            {
                // Обработка ошибки, если добавление не удалось
                return RedirectToAction("Error");
            }
        }



    }
}
