ko.bindingHandlers.inputmask =
{
    init: function (element, valueAccessor, allBindingsAccessor) {

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
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        var mask = valueAccessor();

        var observable = mask.value;

        if (ko.isObservable(observable)) {

            var valuetoWrite = observable();

            $(element).val(valuetoWrite);
        }
    }

};


var IsChange = false;
function CheckChange(obj) {
    var p = ko.computed(function () {
        return ko.toJS(obj);
    });
    p.subscribe(function (value) {
        IsChange = true;
    });
}

function AddDetail(materialCode, materialText, quantity, unitCode, scopeCode, scopeText, timeRent, rentUnitCode, note) {
    model.AddDetail(number, materialCode, materialText, quantity, unitCode, scopeCode, scopeText, timeRent, rentUnitCode, note);
    number = number + 1;
    Forms.CompleteUI();
    ReloadIsRent();
}

function RemoveDetail(obj) {
    model.RemoveDetail($(obj).attr("order"));
}

function AddUser(userName, level, isApprove) {
    debugger;
    model.AddUser(number, userName, level, isApprove);
    number = number + 1;
    Forms.CompleteUI();
}

function RemoveUser(obj) {
    model.RemoveUser($(obj).attr("order"));
}

function AddPR(code, name, type, packageCode, ScopeTypeCode, BidTypeCode, listDetail) {
    model.AddPR(code, name, type, packageCode, ScopeTypeCode, BidTypeCode, listDetail);
}
 
function RemovePR(obj) {
    model.RemovePR($(obj).attr("code"));
}

var Detail = function (order, marterialCode, marterialText, quantity, unitCode, scopeCode, scopeText, timeRent, rentUnitCode, note) {
    var self = this;
    self.ORDER = ko.observable(order);
    self.MATERIAL_CODE = ko.observable(marterialCode);
    self.MATERIAL_TEXT = ko.observable(marterialText);
    self.QUANTITY = ko.observable(quantity);
    self.UNIT_CODE = ko.observable(unitCode);
    self.SCOPE_CODE = ko.observable(scopeCode);
    self.SCOPE_TEXT = ko.observable(scopeText);
    self.TIME_RENT = ko.observable(timeRent);
    self.RENT_UNIT_CODE = ko.observable(rentUnitCode);
    self.NOTE = ko.observable(note);
};

var User = function (order, userName, level, isApprove) {
    var self = this;
    self.ORDER = ko.observable(order);
    self.USER_NAME = ko.observable(userName);
    self.C_LEVEL = ko.observable(level);
    self.IS_APPROVED = ko.observable(isApprove);
};

var PR = function (code, name, type) {
    var self = this;
    self.PR_CODE = ko.observable(code);
    self.TYPE = ko.observable(type);
    self.PR_NAME = ko.observable(name);
    self.ONCLICK = ko.computed(function () {
        return "ShowDetailPR('" + self.TYPE() + "','" + self.PR_CODE() + "')";
    }, this);
    self.ObjListDetail = ko.observableArray([]);
}

var Model = function () {
    var self = this;
    this.ObjHeader = new Object();
    this.ObjListDetail = ko.observableArray([]);
    this.ObjListUser = ko.observableArray([]);
    this.ObjListPR = ko.observableArray([]);
    this.ObjListDetail.removeAll();
    this.ObjListPR.removeAll();
    this.ObjListUser.removeAll();

    /*
    ---------------------------------------------------------------------------------
    - Thêm mới PR
    ---------------------------------------------------------------------------------
    */

    this.AddPR = function (code, name, type, packageCode, ScopeTypeCode, BidTypeCode, listDetail) {
        var find = self.ObjListPR().filter(function (data) {
            return data.PR_CODE() === code; 
        });

        if (find.length == 0) {
            var pr = new PR(code, name, type, packageCode);
            var shouldHide = false;
            $.each(listDetail, function () {
                var detail = this;
                if (this.TIME_RENT == "" || this.RENT_UNIT_CODE == "") {
                    shouldHide = true;
                }
                number = number + 1;
                pr.ObjListDetail.push(new Detail(number, detail.MATERIAL_CODE, detail.MATERIAL_NAME, detail.QUANTITY, detail.UNIT_CODE, detail.SCOPE_CODE, detail.SCOPE_TEXT, detail.TIME_RENT, detail.RENT_UNIT_CODE, detail.NOTE));
                self.AddDetail(number, detail.MATERIAL_CODE, detail.MATERIAL_NAME, detail.QUANTITY, detail.UNIT_CODE, detail.SCOPE_CODE, detail.SCOPE_TEXT, detail.TIME_RENT, detail.RENT_UNIT_CODE, detail.NOTE);
              

            });
            if (shouldHide) {
                $(".rentUnit").hide();
            } else {
                $(".rentUnit").show();
                $(".check_is_rent").prop("checked", true);
            }
            self.ObjListPR.push(pr);
            $("#PACKAGE_CODE").val(packageCode).change();
            $("#SCOPE_TYPE_CODE").val(ScopeTypeCode).change();
            $("#BID_TYPE_CODE").val(BidTypeCode).change();
            $('#REQUIRE_NAME').val(name); 
            $('#projectCodeHidden').val($('#projectDropdown').val());
            $('#projectCodeHidden').val($('#projectDropdown').val());
            $('#projectDropdown').prop('disabled', true);
        }
        Forms.CompleteUI();

    }

    /*
    ---------------------------------------------------------------------------------
    - Xóa PR
    ---------------------------------------------------------------------------------
    */
    this.RemovePR = function (code) {
        var find = self.ObjListPR().filter(function (data) {
            return data.PR_CODE() === code;
        });
        console.log(find.length)

        if (find.length != 0) {
            $.each(find[0].ObjListDetail(), function () {
                debugger
                var detailPR = this;
                var findDetail = self.ObjListDetail().filter(function (data) {
                    return data.MATERIAL_CODE() === detailPR.MATERIAL_CODE();
                });
                if (findDetail.length != 0) {
                    if (findDetail[0].QUANTITY() - detailPR.QUANTITY() > 0) {
                         findDetail[0].QUANTITY(findDetail[0].QUANTITY() - detailPR.QUANTITY());
                     } else {
                         self.ObjListDetail.remove(findDetail[0]);
                     }
                }
            });
            self.ObjListPR.remove(find[0]);
        }
        if ($('#ObjListPR').children().length === 0) {
            $('#projectDropdown').prop('disabled', false);
        }
    };

    /*
    ---------------------------------------------------------------------------------
    - Thêm mới detail
    ---------------------------------------------------------------------------------
    */
    this.AddDetail = function (order, materialCode, materialText, quantity, unitCode, scopeCode, scopeText, timeRent, rentUnitCode,note) {
        //var find = self.ObjListDetail().filter(function (data) {
        //    return data.SCOPE_CODE() === scopeCode && data.MATERIAL_CODE() === materialCode;
        //});

        //if (find.length !== 0) {
        //    // find[0].QUANTITY(find[0].QUANTITY() + quantity);
        //    // find[0].SUM_VALUE(find[0].SUM_VALUE() + sumValue);
        //    return false;
        //}
        //else {
            self.ObjListDetail.push(new Detail(order, materialCode, materialText, quantity, unitCode, scopeCode, scopeText, timeRent, rentUnitCode, note));
            Forms.CompleteUI();
        //}
    };

    /*
    ---------------------------------------------------------------------------------
    - Xóa detail
    ---------------------------------------------------------------------------------
    */
    this.RemoveDetail = function (order) {
        debugger;
        self.ObjListDetail.remove(function (item) {
            return item.ORDER() == order;
        });
    };

    /*
    ---------------------------------------------------------------------------------
    - Thêm mới user
    ---------------------------------------------------------------------------------
    */
    this.AddUser = function (order, userName, level, isApprove) {
        self.ObjListUser.push(new User(order, userName, level, isApprove));
        $("#ic_" + userName).attr("onclick", "");
        Forms.CompleteUI();
    };

    /*
    ---------------------------------------------------------------------------------
    - Xóa user
    ---------------------------------------------------------------------------------
    */
    this.RemoveUser = function (order) {
        debugger;
        self.ObjListUser.remove(function (item) {
            return item.ORDER() == order;
        });
    };
}