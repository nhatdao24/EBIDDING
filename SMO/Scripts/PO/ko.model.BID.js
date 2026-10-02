// 1. BindingHandler cho InputMask
ko.bindingHandlers.inputmask = {
    init: function (element, valueAccessor) {
        var mask = valueAccessor();
        var observable = mask.value;
        if (ko.isObservable(observable)) {
            $(element).on('focusout change', function () {
                if ($(element).inputmask('isComplete')) {
                    observable($(element).val());
                } else {
                    observable(null);
                }
            });
        }
        $(element).inputmask(mask);
    },
    update: function (element, valueAccessor) {
        var mask = valueAccessor();
        var observable = mask.value;
        if (ko.isObservable(observable)) {
            $(element).val(observable());
        }
    }
};

// 2. Constructor Detail
function Detail(code, text, description, quantity, unitCode, price, order) {
    this.CODE = ko.observable(code);
    this.TEXT = ko.observable(text);
    this.DESCRIPTION = ko.observable(description || '');
    this.QUANTITY = ko.observable(quantity || 1);
    this.UNIT = ko.observable(unitCode || '');
    this.PRICE_VTB = ko.observable(price || 0);
    this.ORDER = ko.observable(order);
}

// 3. ViewModel
var Model = function () {
    var self = this;
    self.ObjListDetail = ko.observableArray([]);

    self.AddDetail = function (code, text, description, quantity, unitCode, price) {
        // Kiểm tra trùng mã
        var isExist = ko.utils.arrayFirst(self.ObjListDetail(), function (item) {
            return item.CODE() === code;
        });

        if (isExist) {
            Message.func.AlertDanger({ Message: { Code: "", Message: "Mặt hàng này đã được chọn!" } });
            return false;
        }

        var order = self.ObjListDetail().length + 1;
        self.ObjListDetail.push(new Detail(code, text, description, quantity, unitCode, price, order));

        Message.func.AlertSuccess({ Message: { Code: "", Message: "Thêm thành công." } });
        if (typeof Forms !== 'undefined') { Forms.CompleteUI(); }
    };

    self.RemoveDetail = function (order) {
        self.ObjListDetail.remove(function (item) {
            return item.ORDER() == order;
        });
        // Sắp xếp lại STT (ORDER) sau khi xóa để dữ liệu đồng nhất
        var index = 1;
        ko.utils.arrayForEach(self.ObjListDetail(), function (item) {
            item.ORDER(index++);
        });
    };
    // Thêm vào trong constructor var Model = function () { ... }
    self.TotalValue = ko.computed(function () {
        var total = 0;
        ko.utils.arrayForEach(self.ObjListDetail(), function (item) {
            // Loại bỏ dấu phẩy (nếu có) từ mask-currency trước khi tính toán
            var qty = parseFloat(ko.unwrap(item.QUANTITY).toString().replace(/,/g, '')) || 0;
            var price = parseFloat(ko.unwrap(item.PRICE_VTB).toString().replace(/,/g, '')) || 0;
            total += (qty * price);
        });
        return total;
    });

    // Subscribe để cập nhật giá trị vào ô TextBox (nếu cần xử lý thêm bằng jQuery)
    self.TotalValue.subscribe(function (newValue) {
        // Gán giá trị vào ô TextBox của bạn
        // Chú ý: Sử dụng ID chính xác mà ASP.NET sinh ra (thường là ObjDetail_TOTAL_PRICE)
        $("#ObjDetail_TOTAL_PRICE").val(newValue).trigger("change");

        // Nếu bạn đang dùng mask-currency, cần trigger để nó định dạng lại tiền tệ
        if ($.fn.inputmask) {
            $("#ObjDetail_TOTAL_PRICE").inputmask('setvalue', newValue);
        }
    });
};

// Khởi tạo model global
var model = new Model();

// 4. HÀM GLOBAL (Dành cho Dialog gọi)
window.AddDetail = function (code, name, active, quantity, unitCode, price) {
    model.AddDetail(code, name, '', quantity, unitCode, price);
    // Hiệu ứng icon trên dialog
    var $icon = $('#ic_' + code);
    if ($icon.length > 0) {
        $icon.html('check_circle').css('color', 'green');
    }
};

// 5. HÀM GLOBAL (Dành cho nút Xóa trong Template)
function RemoveDetail(obj) {
    var order = $(obj).attr("order"); // Lấy order từ data-bind attr: {order: ORDER}
    model.RemoveDetail(order);
}
// Giả sử cấu trúc model của bạn đại loại như sau:
//function BidModel() {
//    var self = this;
//    self.ObjListDetail = ko.observableArray([]); // Danh sách các dòng hàng hóa

//    // Thêm hàm tính toán tự động
//    self.ComputedTotalPrice = ko.computed(function () {
//        var total = 0;
//        ko.utils.arrayForEach(self.ObjListDetail(), function (item) {
//            // Chuyển đổi về số để tính toán, tránh lỗi nối chuỗi
//            var qty = parseFloat(ko.unwrap(item.QUANTITY)) || 0;
//            var price = parseFloat(ko.unwrap(item.PRICE_VTB)) || 0;
//            total += (qty * price);
//        });
//        return total;
//    });
//}