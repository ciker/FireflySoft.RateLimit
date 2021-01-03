﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FireflySoft.RateLimit.Core
{
    public class LeakyBucketAlgorithm<TRequest> : IRateLimitAlgorithm<TRequest>
    {
        IEnumerable<LeakyBucketRateLimitRule<TRequest>> _rules;

        public LeakyBucketAlgorithm(IEnumerable<LeakyBucketRateLimitRule<TRequest>> rules)
        {
            _rules = rules;
        }

        public List<RateLimitCheckResult<TRequest>> Check(TRequest request, IRateLimitStorage storage)
        {
            List<RateLimitCheckResult<TRequest>> results = new List<RateLimitCheckResult<TRequest>>();

            foreach (var rule in _rules)
            {
                if (rule.CheckRuleMatching(request))
                {
                    var target = rule.ExtractTarget(request);
                    if (string.IsNullOrWhiteSpace(target))
                    {
                        throw new NotSupportedException("不支持Target为空");
                    }

                    bool result = CheckSingleRule(target, storage, rule);
                    results.Add(new RateLimitCheckResult<TRequest>()
                    {
                        Rule = rule,
                        IsLimit = result
                    });
                }
            }

            return results;
        }

        private bool CheckSingleRule(string target, IRateLimitStorage storage, LeakyBucketRateLimitRule<TRequest> rule)
        {
            if (storage.CheckLocked(target))
            {
                return true;
            }

            var bucketAmount = storage.LeakyBucketIncrement(target, 1, rule.Capacity, (int)rule.OutflowUnit.TotalMilliseconds, rule.OutflowQuantityPerUnit);
            Console.WriteLine("bucketAmount"+bucketAmount);

            if (bucketAmount > rule.Capacity)
            {
                if (rule.LockSeconds > 0)
                {
                    storage.TryLock(target, TimeSpan.FromSeconds(rule.LockSeconds));
                }

                return true;
            }

            return false;
        }
    }
}
