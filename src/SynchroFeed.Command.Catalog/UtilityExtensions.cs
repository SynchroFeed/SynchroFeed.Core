#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="UtilityExtensions.cs">
// MIT License
// 
// Copyright(c) 2018 Robert Vandehey
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Text;
using SynchroFeed.Command.Catalog.Entity;

namespace SynchroFeed.Command.Catalog
{
    /// <summary>
    /// A static utility class that contains some useful extension methods.
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>Gets the validation message associated with a DbEntityValidationException.</summary>
        /// <param name="exception">The exception.</param>
        /// <returns>System.String.</returns>
        public static string GetValidationMessage(this DbEntityValidationException exception)
        {
            var sb = new StringBuilder(1000);
            foreach (var error in exception.EntityValidationErrors)
            {
                sb.AppendLine($"Entity of type \"{error.Entry.Entity.GetType().Name}\" in state \"{error.Entry.State}\" has the following validation errors:");
                foreach (var ve in error.ValidationErrors)
                {
                    sb.AppendLine($"- Property: \"{ve.PropertyName}\", Error: \"{ve.ErrorMessage}\"");
                }
            }

            return sb.ToString();
        }

        /// <summary>An extension method that reverts the changes associated with a PackageModelContext Entity Framework context.</summary>
        /// <param name="context">The Entity Framework context to revert.</param>
        /// <param name="exception">The exception causing the revert.</param>
        public static void RevertChanges(this PackageModelContext context, DbEntityValidationException exception)
        {
            foreach (DbEntityValidationResult item in exception.EntityValidationErrors)
            {
                DbEntityEntry entry = item.Entry;
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
