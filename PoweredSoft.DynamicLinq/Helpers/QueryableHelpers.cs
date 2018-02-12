﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicLinq.Helpers
{
    public static class QueryableHelpers
    {
        public static Expression GetConditionExpressionForMember(ParameterExpression parameter, Expression member, ConditionOperators conditionOperator, ConstantExpression constant)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            if (member == null)
                throw new ArgumentNullException("member");

            if (constant == null)
                throw new ArgumentNullException("constant");

            Expression ret = null;

            if (conditionOperator == ConditionOperators.Equal)
                ret = Expression.Equal(member, constant);
            else if (conditionOperator == ConditionOperators.GreaterThan)
                ret = Expression.GreaterThan(member, constant);
            else if (conditionOperator == ConditionOperators.GreaterThanOrEqual)
                ret = Expression.GreaterThanOrEqual(member, constant);
            else if (conditionOperator == ConditionOperators.LessThan)
                ret = Expression.LessThan(member, constant);
            else if (conditionOperator == ConditionOperators.LessThanOrEqual)
                ret = Expression.LessThanOrEqual(member, constant);
            else if (conditionOperator == ConditionOperators.Contains)
                ret = Expression.Call(member, Constants.ContainsMethod, constant);
            else if (conditionOperator == ConditionOperators.StartsWith)
                ret = Expression.Call(member, Constants.StartsWithMethod, constant);
            else if (conditionOperator == ConditionOperators.EndsWith)
                ret = Expression.Call(member, Constants.EndsWithMethod, constant);
            else
                throw new ArgumentException("conditionOperator", "Must supply a known condition operator");

            return ret;            
        }

        /// <summary>
        /// Returns the right expression for a path supplied.
        /// </summary>
        /// <param name="param">Expression.Parameter(typeOfClassOrInterface)</param>
        /// <param name="path">the path you wish to resolve example Contact.Profile.FirstName</param>
        /// <returns></returns>
        public static Expression ResolvePathForExpression(ParameterExpression param, string path)
        {
            Expression body = param;
            foreach (var member in path.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }
            return body;
        }

        public static ConstantExpression GetConstantSameAsLeftOperator(Expression member, object value)
        {
            if (member == null)
                throw new ArgumentNullException("member");

            if (value == null)
                return Expression.Constant(null);

            // the types.
            var valueType = value.GetType();
            var memberType = member.Type;

            // if match.
            if (valueType == memberType)
                return Expression.Constant(value);

            // attempt a conversion.
            object convertedValue = TypeHelpers.ConvertFrom(memberType, value);
            return Expression.Constant(convertedValue);
        }

        public static ConstantExpression ResolveConstant(Expression member, object value, QueryConvertStrategy convertStrategy)
        {
            if (convertStrategy == QueryConvertStrategy.LeaveAsIs)
                return Expression.Constant(value);

            if (convertStrategy == QueryConvertStrategy.ConvertConstantToComparedPropertyOrField)
                return QueryableHelpers.GetConstantSameAsLeftOperator(member, value);

            throw new NotSupportedException($"{convertStrategy} supplied is not recognized");
        }

        public static IQueryable<T> CreateSortExpression<T>(IQueryable<T> query, string sortPath, SortOrder sortOrder, bool appendSort = true)
        {
            var parameter = Expression.Parameter(typeof(T), "t");
            var member = QueryableHelpers.ResolvePathForExpression(parameter, sortPath);

            string sortCommand = null;
            if (sortOrder == SortOrder.Descending)
                sortCommand = appendSort == false ? "OrderByDescending" : "ThenByDescending";
            else
                sortCommand = appendSort == false ? "OrderBy" : "ThenBy";

            var expression = Expression.Lambda(member, parameter);

            var resultExpression = Expression.Call
                    (typeof(Queryable),
                    sortCommand,
                    new Type[] { typeof(T), member.Type },
                    query.Expression,
                    Expression.Quote(expression)
                );

            query = query.Provider.CreateQuery<T>(resultExpression);
            return query;
        }
    }
}