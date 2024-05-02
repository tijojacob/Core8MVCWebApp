
var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable()
{
    dataTable = $('#myProductTable').DataTable({
        "ajax": { url : '/admin/product/getallproducts'},
        "columns": [
            { data: 'title', "width" : "12.5%" },
            { data: 'description', "width": "40%" },
            { data: 'isbn', "width": "10%" },
            { data: 'price', "width": "2.5%" },
            { data: 'author', "width": "10%" },
            { data: 'category.name', "width": "5%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="btn-group" role="group">
                            <a href="/admin/product/UpsertProduct?id=${data}" class="btn btn-primary">
                                <i class="bi bi-pencil-square"></i>Edit
                            </a>
                            <a onClick=Delete("/admin/product/delete?id=${data}") class="btn btn-danger mx-2 border">
                                <i class="bi bi-trash"></i>Delete
                            </a>
                        </div>`
                }, "width": "20%"
            }
        ]
    });

}

function Delete(Url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            //Swal.fire({
            //    title: "Deleted!",
            //    text: "Your file has been deleted.",
            //    icon: "success"
            //});
            $.ajax({
                url: Url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr["success"](data.message, "Success");
                }
            })
        }
    });
}