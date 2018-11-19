#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="IRepository.cs">
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
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SynchroFeed.Library.Repository
{
    /// <summary>
    /// The interface to implment the Repository pattern.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity the repository stores.</typeparam>
    public interface IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets the name of the repository.
        /// </summary>
        /// <value>The name of the repository.</value>
        string Name { get; }

        /// <summary>
        /// Gets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        string RepositoryType { get; }

        /// <summary>
        /// Adds or updates the specified entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add to the repository.</param>
        void Add(TEntity entity);

        /// <summary>
        /// Deletes the specified entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete from the repository.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Fetches all of the entities from the repository that matches the expression
        /// </summary>
        /// <value>Returns the IEnumerable instance.</value>
        IEnumerable<TEntity> Fetch(Expression<Func<TEntity, bool>> expression = null);

        /// <summary>
        /// Fetches the specified entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to fetch from the repository. The entity is 
        /// used to determine the keys to fetch the entity from the repository.
        /// </param>
        /// <returns>Returns the TEntity or null if the entity is not found.</returns>
        TEntity Fetch(TEntity entity);
    }
}