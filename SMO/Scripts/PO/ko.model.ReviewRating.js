var IsChange = false;
function CheckChange(obj) {
    var p = ko.computed(function () {
        return ko.toJS(obj);
    });
    p.subscribe(function (value) {
        IsChange = true;
    });
}

//function OnChangeSpecType(obj) {
//    $("#divDetailSpec").hide();
//    var type = $(obj).val();
//    if (type == "NUMBER" || type == "SELECT") {
//        $("#divDetailSpec").show();
//    }
//    model.RemoveAllDetail();
//}

//function AddDetail() {
//    model.AddDetail(number, 0, 0, "", "", "", "");
//    number = number + 1;
//    Forms.CompleteUI();
//}

function RemoveDetail(obj) {
    model.RemoveDetail($(obj).attr("order"));
}

var Detail = function (order, fromValue, toValue, rating, pkId, reviewId) {
    var self = this;
    self.C_ORDER = ko.observable(order);
    self.REVIEW_ID = ko.observable(reviewId);
    self.FROM_VALUE = ko.observable(fromValue);
    self.TO_VALUE = ko.observable(toValue);
    self.RATING = ko.observable(rating);
    self.PKID = ko.observable(pkId);
};

var Model = function () {
    var self = this;
    this.ObjDetail = new Object();
    this.ObjList = ko.observableArray([]);
    this.ObjList.removeAll();

    /*
    ---------------------------------------------------------------------------------
    - Thêm mới detail
    ---------------------------------------------------------------------------------
    */
    this.AddDetail = function (order, fromValue, toValue, rating, pkId, reviewId) {
        self.ObjList.push(new Detail(order, fromValue, toValue, rating, pkId, reviewId));
        Forms.CompleteUI();
    };

    /*
    ---------------------------------------------------------------------------------
    - Xóa detail
    ---------------------------------------------------------------------------------
    */
    this.RemoveDetail = function (order) {
        self.ObjList.remove(function (item) {
            return item.C_ORDER() == order;
        });
    };

    /*
    ---------------------------------------------------------------------------------
    - Xóa tất cả detail
    ---------------------------------------------------------------------------------
    */
    this.RemoveAllDetail = function () {
        self.ObjList.removeAll();
    };
}