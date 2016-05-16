(function() {
  define(function(require) {
    var GridHelper;
    return GridHelper = (function() {
      function GridHelper() {
        var j, len, ref, role, roleParent, security, transformRoles;
        transformRoles = function(roles) {
          var j, len, parent, result, role, root, rootId;
          result = [];
          rootId = Math.random();
          root = {
            id: rootId,
            name: "Root"
          };
          result.push(root);
          for (j = 0, len = roles.length; j < len; j++) {
            role = roles[j];
            parent = ko.utils.arrayFirst(result, function(r) {
              return r.name === role.module;
            });
            if (parent == null) {
              parent = {
                id: Math.random(),
                name: role.module,
                module: "Root",
                parentId: rootId
              };
              result.push(parent);
            }
            role.parentId = parent.id;
            result.push(role);
          }
          return result;
        };
        security = require("security/security");
        this.roles = transformRoles(security.operations());
        console.log("transformed roles");
        console.log(this.roles);
        ref = this.roles;
        for (j = 0, len = ref.length; j < len; j++) {
          role = ref[j];
          role.sortkey = role.name.toLowerCase();
          role.level = 0;
          roleParent = role;
          while (roleParent.parentId != null) {
            roleParent = ko.utils.arrayFirst(this.roles, function(r) {
              return r.id === roleParent.parentId;
            });
            role.sortkey = roleParent.name.toLowerCase() + "-" + role.sortkey;
            role.level++;
          }
          role.expanded = true;
        }
        this.roles.sort(function(a, b) {
          if (a.sortkey < b.sortkey) {
            return -1;
          } else if (a.sortkey > b.sortkey) {
            return 1;
          } else {
            return 0;
          }
        });
      }

      GridHelper.prototype.init = function(grid) {
        this.grid = grid;
        return $((function(_this) {
          return function() {
            var checkClickBubblingDownMutex, checkClickBubblingUpMutex, checkboxById, ensureParentCheck, getChildrenIds, getParentId, rowById;
            checkClickBubblingUpMutex = 0;
            checkClickBubblingDownMutex = 0;
            $("input[type=checkbox]", _this.grid).click(function() {
              var checked, itemLevel, level, results, tr;
              tr = $(this).parents("tr");
              level = tr.find("td:eq(1)").text();
              checked = $(this).is(":checked");
              results = [];
              while ((tr = tr.next()).length) {
                itemLevel = tr.find("td:eq(1)").text();
                if (itemLevel <= level) {
                  break;
                }
                results.push($("input[type=checkbox]", tr).prop("checked", checked));
              }
              return results;
            });
            getParentId = function(id) {
              return $("td:eq(1)", rowById(id)).text() || void 0;
            };
            getChildrenIds = function(id) {
              var childrenIds;
              if ((childrenIds = $("td:eq(2)", rowById(id)).text()) !== "") {
                return childrenIds.split(",");
              } else {
                return [];
              }
            };
            rowById = function(id) {
              return $("tr", this.grid).filter(function() {
                return $("td:first", this).text() === id;
              });
            };
            checkboxById = function(id) {
              return $("input[type=checkbox]", rowById(id));
            };
            ensureParentCheck = function(parentId) {
              var allChildrenChecked, childId, parentCheck, parentChecked, parentChildrenIds, parentParentId;
              parentCheck = checkboxById(parentId);
              parentChecked = parentCheck.is(":checked");
              parentChildrenIds = getChildrenIds(parentId);
              allChildrenChecked = ((function() {
                var j, len, results;
                results = [];
                for (j = 0, len = parentChildrenIds.length; j < len; j++) {
                  childId = parentChildrenIds[j];
                  results.push(checkboxById(childId).is(":checked"));
                }
                return results;
              })()).every(function(x) {
                return x;
              });
              if (parentChecked !== allChildrenChecked) {
                checkClickBubblingDownMutex++;
                parentCheck.click();
                checkClickBubblingDownMutex--;
                parentParentId = getParentId(parentId);
                if (parentParentId != null) {
                  return ensureParentCheck(parentParentId);
                }
              }
            };
            $("input[type=checkbox]", _this.grid).each(function() {
              return $(this).addClass($(this).parents("tr").find("td:eq(3)").text());
            });
            return $("input[type=checkbox]:checked", _this.grid).each(function() {
              var id, parentId;
              id = $(this).parents("tr").find("td:first").text();
              if ((parentId = getParentId(id)) != null) {
                return ensureParentCheck(parentId);
              }
            });
          };
        })(this));
      };

      GridHelper.prototype.getChecked = function() {
        var checkedIds;
        checkedIds = [];
        console.log("getChecked");
        $("input[type=checkbox]", this.grid).each(function() {
          var tr;
          tr = $(this).parents("tr");
          if ($(this).is(":checked") && $("td:eq(1)", tr).text() === "2") {
            return checkedIds.push($("td:first", tr).text());
          }
        });
        return checkedIds;
      };

      GridHelper.prototype.reload = function() {
        return this.grid.trigger("reload");
      };

      GridHelper.prototype.reset = function() {
        return $("input[type=checkbox]", this.grid).each(function() {
          return $(this).attr("checked", false);
        });
      };

      GridHelper.prototype.filter = function(pattern) {
        var prevLevel, visibleByLevel;
        pattern = pattern.trim();
        visibleByLevel = [];
        prevLevel = 0;
        return $($("input[type=checkbox]", this.grid).get().reverse()).each(function() {
          var childLevel, childTr, formattedText, html, i, isLeaf, isMatched, isVisible, level, rx, tdText, text, tr;
          tr = $(this).parents("tr");
          if (typeof level !== "undefined" && level !== null) {
            prevLevel = level;
          }
          level = Number(tr.find("td:eq(1)").text());
          tdText = tr.find("td:eq(3)");
          text = tdText.text();
          rx = new RegExp("(" + pattern + ")", "i");
          isMatched = Boolean(text.match(rx));
          isLeaf = level >= prevLevel;
          if (isLeaf) {
            visibleByLevel.length = level + 1;
          }
          isVisible = isLeaf ? isMatched : isMatched || visibleByLevel[level + 1];
          if (isMatched) {
            i = (rx.exec(text)).index;
            formattedText = text.substr(0, i) + "<em>" + text.substr(i, pattern.length) + "</em>" + text.substr(i + pattern.length);
          } else {
            formattedText = text;
          }
          if ($("span+span", tdText).length) {
            $("span+span", tdText).html(formattedText);
          } else {
            html = $("span", tdText).html();
            $(">span", tdText).html(html.substr(0, html.lastIndexOf(">") + 1));
            $(">span", tdText).append($("<span>").html(formattedText));
          }
          if (isMatched && !isLeaf) {
            childTr = tr.next();
            childLevel = Number(childTr.find("td:eq(1)").text());
            while (childTr.length && childLevel > level) {
              childTr.show();
              childTr = childTr.next();
              childLevel = Number(childTr.find("td:eq(1)").text());
            }
          }
          visibleByLevel[level] = isVisible || visibleByLevel[level];
          prevLevel = level;
          return tr.toggle(isVisible);
        });
      };

      return GridHelper;

    })();
  });

}).call(this);

//# sourceMappingURL=grid-helper.js.map
