
var dataTable;
$(document).ready(function () {
    let url = window.location.search;
    let status = "";
    if (url.includes("pending")) {
        status = "pending";
    }
    else if (url.includes("inprocess")) {
        status = "inprocess";
    }
    else if (url.includes("completed")) {
        status = "completed";
    }
    else if (url.includes("approved")) {
        status = "approved";
    }
    else if (url.includes("all")) {
        status = "all";
    }
    loadDataTable(status);
})

function loadDataTable(status)
{
    dataTable = $('#myOrderTable').DataTable({
        "ajax": { url: '/admin/order/GetAllOrders?status=' + status },
        "columns": [
            { data: 'id', "width" : "10%" },
            { data: 'name', "width": "20%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'applicationUser.email', "width": "25%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="btn-group" role="group">
                            <a href="/admin/order/details?orderId=${data}" class="btn btn-primary">
                                <i class="bi bi-pencil-square"></i>
                            </a>                            
                        </div>`
                }, "width": "10%"
            }
        ]
    });

}