using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskManager.Models;
using TaskManager.Application;

namespace TaskManager.API.Testing.Unit
{
    [TestClass]
    public class ValidatorsTests
    {
        [TestMethod]
        public void ToDoListDtoValidator_ValidDto_ReturnsNoErrors()
        {
            var dto = new ToDoListDto { Summary = "Valid summary" };

            var errors = ToDoListDtoValidator.IsValid(dto);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void ToDoListDtoValidator_EmptySummary_ReturnsError()
        {
            var dto = new ToDoListDto { Summary = "" };

            var errors = ToDoListDtoValidator.IsValid(dto);

            Assert.IsTrue(errors.ContainsKey("list.summary.errors"));
            Assert.AreEqual("Summary is empty", errors["list.summary.errors"][0]);
        }

        [TestMethod]
        public void ToDoListDtoValidator_TooLongSummary_ReturnsError()
        {
            var dto = new ToDoListDto { Summary = new string('x', 51) };

            var errors = ToDoListDtoValidator.IsValid(dto);

            Assert.IsTrue(errors.ContainsKey("list.summary.errors"));
            Assert.AreEqual("Summary is too long", errors["list.summary.errors"][0]);
        }

        [TestMethod]
        public void ToDoListDtoValidator_Null_HandledGracefully()
        {
            var errors = ToDoListDtoValidator.IsValid(null);

            Assert.IsTrue(errors.ContainsKey("list.summary.errors"));
            Assert.AreEqual("Unable to save", errors["list.summary.errors"][0]);
        }

        [TestMethod]
        public void ToDoTaskDtoValidator_ValidDto_ReturnsNoErrors()
        {
            var dto = new ToDoTaskDto { Summary = "Valid" };

            var errors = ToDoTaskDtoValidator.IsValid(dto);

            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void ToDoTaskDtoValidator_EmptySummary_ReturnsError()
        {
            var dto = new ToDoTaskDto { Summary = "" };

            var errors = ToDoTaskDtoValidator.IsValid(dto);

            Assert.IsTrue(errors.ContainsKey("list.summary.errors"));
            Assert.AreEqual("Summary is empty", errors["list.summary.errors"][0]);
        }

        [TestMethod]
        public void ToDoTaskDtoValidator_Null_HandledGracefully()
        {
            var errors = ToDoTaskDtoValidator.IsValid(null);

            Assert.IsTrue(errors.ContainsKey("list.summary.errors"));
            Assert.AreEqual("Unable to save", errors["list.summary.errors"][0]);
        }
    }
}